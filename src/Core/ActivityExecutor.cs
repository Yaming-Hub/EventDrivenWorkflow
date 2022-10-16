﻿using Microsoft.EventDrivenWorkflow.Contract;
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
                string payloadType = outputEvent.Payload == null
                    ? typeof(void).FullName
                    : outputEvent.Payload.GetType().FullName;

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
        }

        private async Task ExecuteSync(ActivityExecutionContext activityExecutionContext)
        {
            var aei = activityExecutionContext.ActivityExecutionInfo;
            await using (var activity = this.orchestrator.ActivityFactory.Create(
                partitionKey: aei.PartitionKey, name: aei.ActivityName))
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

            await this.PublishOutputEvents(activityExecutionContext);
        }

        private async Task ExecuteAsync(ActivityExecutionContext activityExecutionContext)
        {
            var aei = activityExecutionContext.ActivityExecutionInfo;
            await using (var activity = this.orchestrator.ActivityFactory.CreateAsync(
                partitionKey: aei.PartitionKey, name: aei.ActivityName))
            {
                try
                {
                    await activity.BeginExecute(activityExecutionContext, CancellationToken.None);
                }
                catch
                {
                    // TODO (ymliu): Handle exceptions.
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