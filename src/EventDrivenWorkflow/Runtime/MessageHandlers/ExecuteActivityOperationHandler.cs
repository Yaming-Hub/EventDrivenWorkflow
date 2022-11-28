// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecuteActivityOperationHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.MessageHandlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EventDrivenWorkflow.Messaging;
    using EventDrivenWorkflow.Runtime.Data;

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

                // Try to load all input event for the activity.
                var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count);
                var allInputEventsAvailable = await orchestrator.InputEventLoader.TryLoadInputEvents(
                    activityDefinition: activityDefinition,
                    workflowExecutionContext: message.WorkflowExecutionContext,
                    triggerEventModel: message.Value.Event,
                    inputEvents: inputEvents);

                if (!allInputEventsAvailable)
                {
                    // This should not happen for retry events.
                    throw new WorkflowRuntimeException(isTransient: false, message: "Input events for a retry event should be available.");
                }

                await orchestrator.ActivityExecutor.Execute(
                    context,
                    activityDefinition,
                    inputEvents: inputEvents,
                    triggerEventModel: message.Value.Event);
            }

            return MessageHandleResult.Complete;
        }
    }
}
