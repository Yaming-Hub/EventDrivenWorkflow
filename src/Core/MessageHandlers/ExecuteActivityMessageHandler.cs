using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.MessageHandlers
{
    internal class ExecuteActivityMessageHandler : ControlMessageHandlerBase
    {
        public ExecuteActivityMessageHandler(WorkflowOrchestrator orchestrator)
            : base(orchestrator)
        {
        }

        protected override ControlOperation Operation => ControlOperation.ExecuteActivity;

        protected override async Task<MessageHandleResult> HandleInternal(ControlMessage message)
        {
            if (string.IsNullOrEmpty(message.TargetActivityName))
            {
                // This is invalid message.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            var workflowDefinition = this.Orchestrator.WorkflowDefinition;
            var activityDefinition = workflowDefinition.ActivityDefinitions
                .FirstOrDefault(a => a.InputEventDefinitions.Any(e => e.Name == message.TargetActivityName));

            if (activityDefinition == null)
            {
                // The target activity is not defined. This may happen if the workflow is changed.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (!activityDefinition.IsInitializing)
            {
                // Only initializing activity can be executed via control message.
                // This may happen if the workflow is changed.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            var wei = message.WorkflowExecutionInfo;
            await this.Orchestrator.ActivityExecutor.Execute(
                workflowDefinition,
                activityDefinition,
                wei,
                inputEvents: new Dictionary<string, EventEntity>());

            return MessageHandleResult.Complete;
        }
    }
}
