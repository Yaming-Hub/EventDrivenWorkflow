// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityExecutor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using System.Text;
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.Model;
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
            IReadOnlyDictionary<string, EventData> inputEvents)
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
                ActivityName = activityDefinition.Name,
                ActivityExecutionStartDateTime = this.orchestrator.Engine.TimeProvider.UtcNow,
                ActivityExecutionId = activityExecutionId
            };


            // Now we got all incoming events for the activity, let's run it.
            var eventOperator = new EventOperator(
                activityDefinition: activityDefinition,
                activityExecutionContext: activityExecutionContext,
                inputEvents: inputEvents);

            eventOperator.ValidateInputEvents();

            if (activityDefinition.IsAsync)
            {
                await this.ExecuteAsync(activityExecutionContext, eventOperator);
            }
            else
            {
                await this.ExecuteSync(activityExecutionContext, eventOperator);
            }
        }

        /// <summary>
        /// Publish output events.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task PublishOutputEvents(ActivityExecutionContext context, EventOperator eventOperator)
        {
            eventOperator.ValidateOutputEvents();

            // Queue the output events.
            foreach (var outputEvent in eventOperator.GetOutputEvents())
            {
                string payloadType = outputEvent.Payload?.GetType()?.FullName;
                byte[] payload = outputEvent.Payload == null
                    ? null
                    : this.orchestrator.Engine.Serializer.Serialize(outputEvent.Payload);

                var message = new EventMessage
                {
                    Id = outputEvent.Id,
                    WorkflowExecutionContext = CopyWorkflowExecutionInfo(context), // Trim the activity part from context
                    EventName = outputEvent.Name,
                    SourceActivityName = context.ActivityName,
                    SourceActivityExecutionId = context.ActivityExecutionId,
                    PayloadType = payloadType,
                    Payload = payload,
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.DelayDuration);
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
        private async Task ExecuteSync(ActivityExecutionContext context, EventOperator eventOperator)
        {
            var activity = this.orchestrator.ActivityFactory.CreateActivity(context.ActivityName);

            try
            {
                await activity.Execute(
                    context: context,
                    eventRetriever: eventOperator,
                    eventPublisher: eventOperator,
                    cancellationToken: CancellationToken.None);
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

            await this.PublishOutputEvents(context, eventOperator);
        }

        /// <summary>
        /// Execute an async activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteAsync(ActivityExecutionContext context, EventOperator eventOperator)
        {
            var activity = this.orchestrator.ActivityFactory.CreateAsyncActivity(context.ActivityName);

            try
            {
                await activity.BeginExecute(
                    context: context,
                    eventRetriever: eventOperator,
                    cancellationToken: CancellationToken.None);
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
                WorkflowStartDateTime = workflowExecutionContext.WorkflowStartDateTime
            };
        }

        /// <summary>
        /// Calculate activity execution id based on input events.
        /// </summary>
        /// <param name="inputEvents">The input events.</param>
        /// <returns>The calculated activity execution id.</returns>
        private static Guid CaculateActivityExecutionId(string activityName, IReadOnlyDictionary<string, EventData> inputEvents)
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
