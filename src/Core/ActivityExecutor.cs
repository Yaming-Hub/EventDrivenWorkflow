using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Core.Messaging;
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
            IReadOnlyDictionary<string, Event> inputEvents)
        {
            // Now we got all incoming events for the activity, let's run it.
            var activityExecutionContext = new ActivityExecutionContext(
                workflowDefinition: workflowDefinition,
                activityDefinition: activityDefinition,
                workflowExecutionInfo: workflowExecutionInfo,
                activityExecutionId: CaculateActivityExecutionId(inputEvents),
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
                    WorkflowExecutionInfo = workflowExecutionInfo,
                    EventName = outputEvent.Name,
                    SourceActivityName = activityDefinition.Name,
                    SourceActivityExecutionId = activityExecutionContext.ActivityExecutionId,
                    PayloadType = payloadType,
                    Payload = payload,
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.Delay);
            }
        }

        private static Guid CaculateActivityExecutionId(IReadOnlyDictionary<string, Event> inputEvents)
        {
            throw new NotImplementedException();
        }
    }
}
