using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    internal class ActivityExecutor
    {
        private readonly WorkflowOrchestrator orchestrator;

        public ActivityExecutor(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        public async Task Execute(
            WorkflowDefinition workflowDefinition,
            ActivityDefinition activityDefinition,
            WorkflowExecutionInfo workflowExecutionInfo,
            IReadOnlyDictionary<string, EventData> inputEvents)
        {
            var activityExecutionId = CaculateActivityExecutionId(inputEvents);
            var activityExecutionInfo = BuildActivtyExecutionInfo(workflowExecutionInfo, activityDefinition, activityExecutionId);

            // Now we got all incoming events for the activity, let's run it.
            var activityExecutionContext = new ActivityExecutionContext(
                workflowDefinition: workflowDefinition,
                activityDefinition: activityDefinition,
                activityExecutionInfo: activityExecutionInfo,
                inputEvents: inputEvents);

            activityExecutionContext.ValidateInputEvents();

            if (activityDefinition.IsAsync)
            {
                await this.ExecuteAsync(activityExecutionContext);
            }
            else
            {
                await this.ExecuteSync(activityExecutionContext);
            }
        }

        public async Task PublishOutputEvents(ActivityExecutionContext activityExecutionContext)
        {
            activityExecutionContext.ValidateOutputEvents();
            var aei = activityExecutionContext.ActivityExecutionInfo;

            // Queue the output events.
            foreach (var outputEvent in activityExecutionContext.GetOutputEvents())
            {
                string payloadType = outputEvent.Payload?.GetType()?.FullName;
                byte[] payload = outputEvent.Payload == null
                    ? null
                    : this.orchestrator.Engine.Serializer.Serialize(outputEvent.Payload);

                var message = new EventMessage
                {
                    Id = outputEvent.Id,
                    WorkflowExecutionInfo = CopyWorkflowExecutionInfo(aei),
                    EventName = outputEvent.Name,
                    SourceActivityName = aei.ActivityName,
                    SourceActivityExecutionId = aei.ActivityExecutionId,
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

        private async Task ExecuteSync(ActivityExecutionContext activityExecutionContext)
        {
            var aei = activityExecutionContext.ActivityExecutionInfo;

            var activity = this.orchestrator.ActivityFactory.CreateActivity(aei.ActivityName);

            try
            {
                await activity.Execute(activityExecutionContext, CancellationToken.None);
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

            await this.PublishOutputEvents(activityExecutionContext);
        }

        private async Task ExecuteAsync(ActivityExecutionContext activityExecutionContext)
        {
            var aei = activityExecutionContext.ActivityExecutionInfo;

            var activity = this.orchestrator.ActivityFactory.CreateAsyncActivity(aei.ActivityName);

            try
            {
                await activity.BeginExecute(activityExecutionContext, CancellationToken.None);
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

        private static ActivityExecutionInfo BuildActivtyExecutionInfo(
            WorkflowExecutionInfo workflowExecutionInfo,
            ActivityDefinition activityDefinition,
            Guid activityExecutionId)
        {
            return new ActivityExecutionInfo
            {
                PartitionKey = workflowExecutionInfo.PartitionKey,
                WorkflowName = workflowExecutionInfo.WorkflowName,
                WorkflowVersion = workflowExecutionInfo.WorkflowVersion,
                WorkflowId = workflowExecutionInfo.WorkflowId,
                WorkflowStartDateTime = workflowExecutionInfo.WorkflowStartDateTime,
                ActivityName = activityDefinition.Name,
                ActivityStartDateTime = DateTime.UtcNow,
                ActivityExecutionId = activityExecutionId
            };
        }

        private static WorkflowExecutionInfo CopyWorkflowExecutionInfo(WorkflowExecutionInfo activityExecutionInfo)
        {
            return new WorkflowExecutionInfo
            {
                PartitionKey = activityExecutionInfo.PartitionKey,
                WorkflowName = activityExecutionInfo.WorkflowName,
                WorkflowVersion = activityExecutionInfo.WorkflowVersion,
                WorkflowId = activityExecutionInfo.WorkflowId,
                WorkflowStartDateTime = activityExecutionInfo.WorkflowStartDateTime
            };
        }

        private static Guid CaculateActivityExecutionId(IReadOnlyDictionary<string, EventData> inputEvents)
        {
            throw new NotImplementedException();
        }
    }
}
