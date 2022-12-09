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
        public async Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, ControlModel controlModel)
        {
            var model = (ExecuteActivityControlModel)controlModel;

            if (string.IsNullOrEmpty(model.TargetActivityName))
            {
                // This is invalid message.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (!orchestrator.WorkflowDefinition.ActivityDefinitions.TryGetValue(
                model.TargetActivityName, out var activityDefinition))
            {
                // The target activity is not defined. This may happen if the workflow is changed.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (model.ExecutionContext.ActivityExecutionContext == null)
            {
                // New execution.
                await orchestrator.ActivityExecutor.Execute(
                    model.ExecutionContext.WorkflowExecutionContext,
                    activityDefinition,
                    inputEvents: new Dictionary<string, Event>(),
                    triggerEvent: model.Event);
            }
            else
            {
                // Retry.
                var context = new ExecutionContext
                {
                    WorkflowExecutionContext = model.ExecutionContext.WorkflowExecutionContext,
                    ActivityExecutionContext = model.ExecutionContext.ActivityExecutionContext
                };

                // Try to load all input event for the activity.
                var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count);
                var allInputEventsAvailable = await orchestrator.InputEventLoader.TryLoadInputEvents(
                    activityDefinition: activityDefinition,
                    workflowExecutionContext: model.ExecutionContext.WorkflowExecutionContext,
                    triggerEventModel: model.Event,
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
                    triggerEventModel: model.Event);
            }

            return MessageHandleResult.Complete;
        }
    }
}
