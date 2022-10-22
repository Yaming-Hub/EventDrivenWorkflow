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
        /// <param name="eventModel">The triggering event model.</param>
        /// <param name="inputEvents">A dictionary contains input events.</param>
        /// <returns>A task represents the async operation.</returns>
        public Task Execute(
            WorkflowExecutionContext workflowExecutionContext,
            ActivityDefinition activityDefinition,
            IReadOnlyDictionary<string, Event> inputEvents,
            EventModel triggerEvent)
        {
            // Construct activity execution context.
            var context = new ExecutionContext
            {
                WorkflowExecutionContext = workflowExecutionContext,
                ActivityExecutionContext = new ActivityExecutionContext
                {
                    ActivityName = activityDefinition.Name,
                    ActivityExecutionStartDateTime = this.orchestrator.Engine.TimeProvider.UtcNow,
                    ActivityExecutionId = CaculateActivityExecutionId(activityDefinition.Name, inputEvents),
                    AttemptCount = 0
                }
            };

            return this.Execute(context, activityDefinition, inputEvents, triggerEvent);
        }


        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="wec">The workflow execution context.</param>
        /// <param name="activityDefinition">The activity definition.</param>
        /// <param name="eventModel">The triggering event model.</param>
        /// <param name="inputEvents">A dictionary contains input events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task Execute(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            IReadOnlyDictionary<string, Event> inputEvents,
            EventModel triggerEvent)
        {
            // Now we got all incoming events for the activity, let's run it.
            var eventOperator = new EventOperator(
                this.orchestrator,
                activityDefinition: activityDefinition,
                context: context,
                inputEvents: inputEvents);

            eventOperator.ValidateInputEvents();

            if (activityDefinition.IsAsync)
            {
                await this.ExecuteAsync(
                    context,
                    activityDefinition,
                    eventOperator,
                    triggerEvent);
            }
            else
            {
                await this.ExecuteSync(
                    context,
                    activityDefinition,
                    eventOperator,
                    triggerEvent);
            }
        }

        /// <summary>
        /// Publish output events.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task PublishOutputEvents(
            ExecutionContext context,
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
                        },
                        SourceActivity = new ActivityReference
                        {
                            Name = context.ActivityExecutionContext.ActivityName,
                            ExecutionId = context.ActivityExecutionContext.ActivityExecutionId,
                        }
                    },
                    WorkflowExecutionContext = context.WorkflowExecutionContext, // Trim the activity part from context
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.DelayDuration);

                await this.orchestrator.Engine.Observer.EventPublished(context, outputEvent);
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
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteSync(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator,
            EventModel triggerEvent)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateExecutable(context.ActivityExecutionContext.ActivityName);

            await this.orchestrator.Engine.Observer.ActivityStarting(context, eventOperator.GetInputEvents());

            var cancellationTokenSource = new CancellationTokenSource(delay: activityDefinition.MaxExecuteDuration);

            bool succeeded = false;

            try
            {
                await activity.Execute(
                    context,
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
                if (context.ActivityExecutionContext.AttemptCount < activityDefinition.RetryPolicy.MaxRetryCount)
                {
                    var controlMessage = new Message<ControlModel>
                    {
                        Value = new ControlModel
                        {
                            Event = triggerEvent,
                            Operation = ControlOperation.ExecuteActivity,
                            TargetActivityName = activityDefinition.Name,
                            ActivityExecutionContext = IncrementAttemptCount(context.ActivityExecutionContext),
                        },
                        WorkflowExecutionContext = context.WorkflowExecutionContext,
                    };

                    await this.orchestrator.Engine.ControlMessageSender.Send(controlMessage, activityDefinition.RetryPolicy.DelayDuration);
                }
                else
                {
                    // Max retry count limit is reached.
                    // TODO: Abandon activity.
                }
            }
        }

        /// <summary>
        /// Execute an async activity.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteAsync(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator,
            EventModel triggerEvent)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateAsyncExecutable(context.ActivityExecutionContext.ActivityName);
            await this.orchestrator.Engine.ActivityExecutionContextStore.Upsert(
                partitionKey: context.WorkflowExecutionContext.PartitionKey,
                key: context.QualifiedExecutionId.ToString(),
                new Entity<ExecutionContext>
                {
                    Value = context,
                    ExpireDateTime = context.WorkflowExecutionContext.WorkflowExpireDateTime,
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

        private static ActivityExecutionContext IncrementAttemptCount(ActivityExecutionContext activityExecutionContext)
        {
            return new ActivityExecutionContext
            {
                ActivityExecutionStartDateTime = activityExecutionContext.ActivityExecutionStartDateTime,
                ActivityName = activityExecutionContext.ActivityName,
                ActivityExecutionId = activityExecutionContext.ActivityExecutionId,

                AttemptCount = activityExecutionContext.AttemptCount + 1,
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
