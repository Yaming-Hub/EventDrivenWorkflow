// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventMessageHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.MessageHandlers
{
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Messaging;
    using EventDrivenWorkflow.Persistence;
    using EventDrivenWorkflow.Runtime.Data;
    using EventDrivenWorkflow.Utilities;

    /// <summary>
    /// This class defines a message handler which handles event messages.
    /// </summary>
    internal sealed class EventMessageHandler : IMessageHandler<EventMessage>
    {
        /// <summary>
        /// The workflow ochestrator.
        /// </summary>
        private readonly WorkflowOrchestrator orchestrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventMessageHandler"/> class.
        /// </summary>
        /// <param name="orchestrator">The workflow ochestrator.</param>
        public EventMessageHandler(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        /// <inheritdoc/>
        public async Task<MessageHandleResult> Handle(EventMessage message)
        {
            try
            {
                return await HandleInternal(message);
            }
            catch (WorkflowRuntimeException wre)
            {
                await this.orchestrator.Engine.Observer.HandleEventMessageFailed(wre, message);
                return wre.IsTransient ? MessageHandleResult.Yield : MessageHandleResult.Complete;
            }
            catch (Exception e)
            {
                await this.orchestrator.Engine.Observer.HandleEventMessageFailed(e, message);
                return MessageHandleResult.Yield; // Unknown exception will be considered as traisent.
            }
        }

        private async Task<MessageHandleResult> HandleInternal(EventMessage message)
        {
            var workflowDefinition = orchestrator.WorkflowDefinition;
            if (message.WorkflowExecutionContext.WorkflowName != workflowDefinition.Name)
            {
                // The event doesn't belong to this workflow, it should be handled by another handler.
                return MessageHandleResult.Continue; // Ignore if the event doesn't belong to this workflow.
            }

            if (!workflowDefinition.EventDefinitions.TryGetValue(message.EventModel.Name, out var eventDefinition))
            {
                // Got an unknown event. This may happen if the workflow is changed.
                throw new WorkflowRuntimeException(
                    isTransient: false,
                    $"Event {message.EventModel.Name}[ver:{message.WorkflowExecutionContext.WorkflowVersion}] is not defined in the " +
                    $"workflow {this.orchestrator.WorkflowDefinition.GetNameAndVersion()}.");
            }

            if (eventDefinition.PayloadType?.FullName != message.EventModel.Payload?.TypeName)
            {
                // The incoming event payload type doesn't match the event definition, the workflow logic may have been changed.
                throw new WorkflowRuntimeException(
                    isTransient: false,
                    $"Event {message.EventModel.Name}[ver:{message.WorkflowExecutionContext.WorkflowVersion}] payload type " +
                    $"{message.EventModel.Payload.TypeName ?? "<null>"} doesn't match event definition {eventDefinition.PayloadType.GetDisplayName()} " +
                    $"of workflow {this.orchestrator.WorkflowDefinition.GetNameAndVersion()}.");
            }

            // Find the activity that subscribe to the current event.
            var activityDefinition = workflowDefinition.EventToConsumerActivityMap[message.EventModel.Name];

            // Try to load all input event for the activity.
            var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count);
            var allInputEventsAvailable = await this.orchestrator.InputEventLoader.TryLoadInputEvents(
                activityDefinition: activityDefinition,
                workflowExecutionContext: message.WorkflowExecutionContext,
                triggerEventModel: message.EventModel,
                inputEvents: inputEvents);

            if (!allInputEventsAvailable)
            {
                // The activity is not yet ready to execute.
                return MessageHandleResult.Complete;
            }

            // Execute the activity when all inputs are ready.
            var context = await this.orchestrator.ActivityExecutor.Execute(
                message.WorkflowExecutionContext,
                activityDefinition,
                inputEvents,
                triggerEvent: message.EventModel);

            // Delete the trigger event presence after execution completes successfully.
            if (message.WorkflowExecutionContext.Options.TrackProgress)
            {
                var partitionKey = ResourceKeyFormat.GetWorkflowPartition(context.WorkflowExecutionContext);
                await this.orchestrator.Engine.EventPresenseStore.Delete(
                    partitionKey: partitionKey,
                    key: ResourceKeyFormat.GetEventKey(
                        partitionKey: context.WorkflowExecutionContext.PartitionKey,
                        workflowName: context.WorkflowExecutionContext.WorkflowName,
                        workflowId: context.WorkflowExecutionContext.WorkflowId,
                        eventName: message.EventModel.Name,
                        eventId: message.EventModel.Id));
            }

            return MessageHandleResult.Complete;
        }
    }
}
