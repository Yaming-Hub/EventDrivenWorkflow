// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecuteActivityOperationHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    internal sealed class ExecuteActivityOperationHandler : IControlOperationHandler
    {
        public async Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, Message<ControlModel> message)
        {
            if (string.IsNullOrEmpty(message.Value.TargetActivityName))
            {
                // This is invalid message.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (!orchestrator.WorkflowDefinition.ActivityDefinitions.TryGetValue(
                message.Value.TargetActivityName, out var activityDefinition))
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

            if (message.Value.ActivityExecutionContext == null)
            {
                // New execution.
                await orchestrator.ActivityExecutor.Execute(
                    message.WorkflowExecutionContext,
                    activityDefinition,
                    inputEvents: new Dictionary<string, Event>(),
                    triggerEvent: message.Value.Event);
            }
            else
            {
                // Retry.
                var context = new ExecutionContext
                {
                    WorkflowExecutionContext = message.WorkflowExecutionContext,
                    ActivityExecutionContext = message.Value.ActivityExecutionContext
                };

                await orchestrator.ActivityExecutor.Execute(
                    context,
                    activityDefinition,
                    inputEvents: new Dictionary<string, Event>(),
                    triggerEvent: message.Value.Event);
            }

            return MessageHandleResult.Complete;
        }
    }
}
