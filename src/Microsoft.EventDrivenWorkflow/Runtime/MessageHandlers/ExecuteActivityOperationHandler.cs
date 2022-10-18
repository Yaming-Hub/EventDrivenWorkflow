﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    internal class ExecuteActivityOperationHandler : IControlOperationHandler
    {
        public async Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, ControlMessage message)
        {
            if (string.IsNullOrEmpty(message.TargetActivityName))
            {
                // This is invalid message.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (!orchestrator.WorkflowDefinition.ActivityDefinitions.TryGetValue(
                message.TargetActivityName, out var activityDefinition))
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

            await orchestrator.ActivityExecutor.Execute(
                message.WorkflowExecutionContext,
                activityDefinition,
                inputEvents: new Dictionary<string, EventData>());

            return MessageHandleResult.Complete;
        }
    }
}