using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
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

            await using (var activity = this.orchestrator.ActivityFactory.Create(
                partitionKey: workflowExecutionInfo.PartitionKey, name: activityDefinition.Name))
            {
                try
                {
                    await activity.Execute(activityExecutionContext, CancellationToken.None);
                }
                catch
                {
                    // TODO (ymliu): Handle exceptions.
                }
            }

            activityExecutionContext.ValidateOutputEvents();

            // Queue the output events.
            foreach (var outputEvent in activityExecutionContext.GetOutputEvents())
            {
                string payloadType = outputEvent.Payload == null
                    ? typeof(void).FullName
                    : outputEvent.Payload.GetType().FullName;

                byte[] payload = outputEvent.Payload == null
                    ? null
                    : this.orchestrator.Engine.Serializer.Serialize(outputEvent.Payload);

                var message = new EventMessage
                {
                    Id = outputEvent.Id,
                    WorkflowExecutionInfo = workflowExecutionInfo,
                    EventName = outputEvent.Name,
                    SourceActivityName = activityDefinition.Name,
                    SourceActivityExecutionId = activityExecutionContext.ActivityExecutionInfo.ActivityExecutionId,
                    PayloadType = payloadType,
                    Payload = payload,
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.DelayDuration);
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

        private static Guid CaculateActivityExecutionId(IReadOnlyDictionary<string, EventData> inputEvents)
        {
            throw new NotImplementedException();
        }
    }
}
