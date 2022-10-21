// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityExecutor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using System.Text;
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;
    using Microsoft.EventDrivenWorkflow.Utilities;

    /// <summary>
    /// This class defines the activity executor.
    /// </summary>
    internal sealed class ActivityExecutor
    {
        /// <summary>
        /// The workflow orchestrator.
        /// </summary>
        private readonly WorkflowOrchestrator orchestrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityExecutor"/> class.
        /// </summary>
        /// <param name="orchestrator">The workflow orchestrator.</param>
        public ActivityExecutor(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="workflowExecutionContext">The workflow execution context.</param>
        /// <param name="activityDefinition">The activity definition.</param>
        /// <param name="inputEvents">A dictionary contains input events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task Execute(
            WorkflowExecutionContext workflowExecutionContext,
            ActivityDefinition activityDefinition,
            IReadOnlyDictionary<string, Event> inputEvents)
        {
            // Construct activity execution id and context.
            var activityExecutionId = CaculateActivityExecutionId(activityDefinition.Name, inputEvents);
            var activityExecutionContext = new ActivityExecutionContext
            {
                PartitionKey = workflowExecutionContext.PartitionKey,
                WorkflowName = workflowExecutionContext.WorkflowName,
                WorkflowVersion = workflowExecutionContext.WorkflowVersion,
                WorkflowId = workflowExecutionContext.WorkflowId,
                WorkflowStartDateTime = workflowExecutionContext.WorkflowStartDateTime,
                WorkflowExpireDateTime = workflowExecutionContext.WorkflowExpireDateTime,
                Options = workflowExecutionContext.Options,
                ActivityName = activityDefinition.Name,
                ActivityExecutionStartDateTime = this.orchestrator.Engine.TimeProvider.UtcNow,
                ActivityExecutionId = activityExecutionId
            };


            // Now we got all incoming events for the activity, let's run it.
            var eventOperator = new EventOperator(
                this.orchestrator,
                activityDefinition: activityDefinition,
                activityExecutionContext: activityExecutionContext,
                inputEvents: inputEvents);

            eventOperator.ValidateInputEvents();

            if (activityDefinition.IsAsync)
            {
                await this.ExecuteAsync(activityExecutionContext, activityDefinition, eventOperator);
            }
            else
            {
                await this.ExecuteSync(activityExecutionContext, activityDefinition, eventOperator);
            }
        }

        /// <summary>
        /// Publish output events.
        /// </summary>
        /// <param name="activityExecutionContext">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task PublishOutputEvents(
            ActivityExecutionContext activityExecutionContext,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator)
        {
            eventOperator.ValidateOutputEvents();

            // Queue the output events.
            foreach (var outputEvent in eventOperator.GetOutputEvents())
            {
                var eventDefinition = activityDefinition.OutputEventDefinitions[outputEvent.Name];
                var payloadType = eventDefinition.PayloadType;
                object payload = outputEvent.GetPayload(payloadType);

                var message = new Message<EventModel>
                {
                    Value = new EventModel
                    {
                        Id = outputEvent.Id,
                        SourceEngineId = outputEvent.SourceEngineId,
                        Name = outputEvent.Name,
                        Payload = new Payload
                        {
                            TypeName = payloadType?.FullName,
                            Body = payload == null ? null : this.orchestrator.Engine.Serializer.Serialize(payload),
                        }
                    },
                    WorkflowExecutionContext = CopyWorkflowExecutionInfo(activityExecutionContext), // Trim the activity part from context
                    SourceActivity = new ActivityReference
                    {
                        Name = activityExecutionContext.ActivityName,
                        ExecutionId = activityExecutionContext.ActivityExecutionId,
                    }
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.DelayDuration);

                await this.orchestrator.Engine.Observer.EventPublished(activityExecutionContext, outputEvent);
            }

            // TODO: If the workflow is being tracked and there is no output event published
            //       by the current activity. This is one terminating operation, and we should
            //       check if there is any pending events. If there is no pending events other
            //       than the current one, then we can consider workflow as completed. This 
            //       should also work for async activity.
        }

        /// <summary>
        /// Execute a sync activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteSync(ActivityExecutionContext context, ActivityDefinition activityDefinition, EventOperator eventOperator)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateExecutable(context.ActivityName);

            await this.orchestrator.Engine.Observer.ActivityStarting(context, eventOperator.GetInputEvents());

            var cancellationTokenSource = new CancellationTokenSource(delay: activityDefinition.MaxExecuteDuration);

            bool succeeded = false;

            try
            {
                await activity.Execute(
                    context: context,
                    eventRetriever: eventOperator,
                    eventPublisher: eventOperator,
                    cancellationToken: cancellationTokenSource.Token);

                succeeded = true;

                await this.orchestrator.Engine.Observer.ActivityCompleted(context, eventOperator.GetOutputEvents());
            }
            catch (OperationCanceledException oce) when (oce.CancellationToken == cancellationTokenSource.Token)
            {
                await this.orchestrator.Engine.Observer.ActivityExecutionTimeout(context);

                // TODO: Activity timeout
            }
            catch (Exception e)
            {
                await this.orchestrator.Engine.Observer.ActivityExecutionFailed(e, context);

                // TODO: Handle exceptions.
            }
            finally
            {
                if (activity is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
                else if (activity is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
            }

            if (succeeded)
            {
                await this.PublishOutputEvents(context, activityDefinition, eventOperator);
            }
            else
            {
                // Retry.
            }
        }

        /// <summary>
        /// Execute an async activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteAsync(ActivityExecutionContext context, ActivityDefinition activityDefinition, EventOperator eventOperator)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateAsyncExecutable(context.ActivityName);

            await this.orchestrator.Engine.ActivityExecutionContextStore.Upsert(
                partitionKey: context.PartitionKey,
                key: context.QualifiedExecutionId.ToString(),
                new Entity<ActivityExecutionContext>
                {
                    Value = context,
                    ExpireDateTime = context.WorkflowExpireDateTime,
                });

            try
            {
                await this.orchestrator.Engine.Observer.ActivityStarting(context, eventOperator.GetInputEvents());

                await activity.BeginExecute(
                    context: context,
                    eventRetriever: eventOperator);
            }
            catch
            {
                // TODO (ymliu): Handle exceptions.
            }
            finally
            {
                if (activity is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
                else if (activity is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
            }
        }

        /// <summary>
        /// Copy workflow execution context.
        /// </summary>
        /// <param name="workflowExecutionContext">The source workflow execution context.</param>
        /// <returns>The copied workflow execution context.</returns>
        private static WorkflowExecutionContext CopyWorkflowExecutionInfo(WorkflowExecutionContext workflowExecutionContext)
        {
            return new WorkflowExecutionContext
            {
                PartitionKey = workflowExecutionContext.PartitionKey,
                WorkflowName = workflowExecutionContext.WorkflowName,
                WorkflowVersion = workflowExecutionContext.WorkflowVersion,
                WorkflowId = workflowExecutionContext.WorkflowId,
                WorkflowStartDateTime = workflowExecutionContext.WorkflowStartDateTime,
                WorkflowExpireDateTime = workflowExecutionContext.WorkflowExpireDateTime,
                Options = workflowExecutionContext.Options,
            };
        }

        /// <summary>
        /// Calculate activity execution id based on input events.
        /// </summary>
        /// <param name="inputEvents">The input events.</param>
        /// <returns>The calculated activity execution id.</returns>
        private static Guid CaculateActivityExecutionId(string activityName, IReadOnlyDictionary<string, Event> inputEvents)
        {
            var sb = new StringBuilder(capacity: activityName.Length + 1 + 37 * inputEvents.Count);
            sb.Append(activityName).Append(":");
            foreach (var id in inputEvents.Values.Select(e => e.Id).OrderBy(x => x))
            {
                sb.Append(id).Append(";");
            }

            return MurmurHash3.HashToGuid(sb.ToString());
        }
    }
}
