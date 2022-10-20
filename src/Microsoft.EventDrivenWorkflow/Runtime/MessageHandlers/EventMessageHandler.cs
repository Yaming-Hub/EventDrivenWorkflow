// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventMessageHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Persistence;
    using Microsoft.EventDrivenWorkflow.Runtime.Model;

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

            if (!workflowDefinition.EventDefinitions.TryGetValue(message.EventName, out var eventDefinition))
            {
                // Got an unknown event. This may happen if the workflow is changed.
                throw new WorkflowRuntimeException(
                    isTransient: false,
                    $"Event {message.EventName}[ver:{message.WorkflowExecutionContext.WorkflowVersion}] is not defined in the " +
                    $"workflow {this.orchestrator.WorkflowDefinition.GetNameAndVersion()}.");
            }

            if (eventDefinition.PayloadType?.FullName != message.PayloadType)
            {
                // The incoming event payload type doesn't match the event definition, the workflow logic may have been changed.
                throw new WorkflowRuntimeException(
                    isTransient: false,
                    $"Event {message.EventName}[ver:{message.WorkflowExecutionContext.WorkflowVersion}] payload type "+ 
                    $"{message.PayloadType ?? "<null>"} doesn't match event definition {eventDefinition.PayloadType.GetDisplayName()} " +
                    $"of workflow {this.orchestrator.WorkflowDefinition.GetNameAndVersion()}.");
            }

            // Find the activity that subscribe to the current event.
            var activityDefinition = workflowDefinition.EventToSubscribedActivityMap[message.EventName];

            var @event = MapToEvent(message, eventDefinition.PayloadType);
            var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count)
            {
                [message.EventName] = @event
            };

            if (activityDefinition.InputEventDefinitions.Count > 1)
            {
                // If the activity subscribes to multiple events, then it cannot be triggered by a single event.
                // To solve this problem, an extra activity state is being introduced to track the input event
                // availability. The events are also persisted in an external storage. Try to load other input 
                // events and update activity states to see if the activity is ready to execute.
                var allInputEventsAvailable = await TryLoadInputEvents(
                    activityDefinition,
                    message.WorkflowExecutionContext,
                    message,
                    @event,
                    inputEvents);

                if (!allInputEventsAvailable)
                {
                    // The activity is not yet ready to execute.
                    return MessageHandleResult.Complete;
                }
            }

            // Execute the activity when all inputs are ready.
            await this.orchestrator.ActivityExecutor.Execute(
                message.WorkflowExecutionContext,
                activityDefinition,
                inputEvents);

            return MessageHandleResult.Complete;
        }

        private async Task<bool> TryLoadInputEvents(
            ActivityDefinition activityDefinition,
            WorkflowExecutionContext wec,
            EventMessage message,
            Event @event,
            Dictionary<string, Event> inputEvents)
        {
            var activityKey = GetActivityKey(wec.WorkflowName, wec.WorkflowId, activityDefinition.Name, wec.PartitionKey);
            var eventKey = GetEventKey(wec.WorkflowName, wec.WorkflowId, @event.Name, wec.PartitionKey);

            var activity = await orchestrator.Engine.ActivityStateStore.GetOrAdd(
                wec.PartitionKey,
                activityKey,
                () => new ActivityStateEntity
                {
                    WorkflowName = wec.WorkflowName,
                    WorkflowVersion = wec.WorkflowVersion,
                    WorkflowId = wec.WorkflowId,
                    Name = activityDefinition.Name,
                    AvailableInputEvents = new List<string> { @event.Name }
                });

            // Persist event to store. The event may have already been persisted in case of recurrent activity, here always
            // override existing event.
            var eventEntity = new EventEntity
            {
                Id = @event.Id,
                Name = @event.Name,
                SourceEngineId = @event.SourceEngineId,
                DelayDuration = @event.DelayDuration,
                PayloadType = message.PayloadType,
                Payload = message.Payload,
            };

            await orchestrator.Engine.EventStore.Upsert(wec.PartitionKey, eventKey, eventEntity);

            if (!activity.AvailableInputEvents.Contains(@event.Name))
            {
                // The event is not in the list, that means the activity is fetched from store. Add the event to
                // activity and try to persist it.
                activity.AvailableInputEvents.Add(@event.Name);

                try
                {
                    await orchestrator.Engine.ActivityStateStore.Update(wec.PartitionKey, activityKey, activity);
                }
                catch (StoreException se) when (se.ErrorCode == StoreErrorCode.EtagMismatch)
                {
                    // The activity has been updated by another orchestrator. Re-load the activity and try again.
                    // Please note, the code will only try one more time here, if the update operation continue to 
                    // fail, then let the orchestrator to handle it.
                    activity = await orchestrator.Engine.ActivityStateStore.Get(wec.PartitionKey, activityKey);
                    if (!activity.AvailableInputEvents.Contains(@event.Name))
                    {
                        activity.AvailableInputEvents.Add(@event.Name);
                    }

                    await orchestrator.Engine.ActivityStateStore.Update(wec.PartitionKey, activityKey, activity);
                }
            }

            if (activity.AvailableInputEvents.Count == activityDefinition.InputEventDefinitions.Count)
            {
                var otherEventKeys = activity.AvailableInputEvents
                    .Where(n => n != @event.Name)
                    .Select(n => GetEventKey(wec.WorkflowName, wec.WorkflowId, n, wec.PartitionKey))
                    .ToList();

                var otherEventEntities = await orchestrator.Engine.EventStore.GetMany(wec.PartitionKey, otherEventKeys);

                foreach (var otherEventEntity in otherEventEntities)
                {
                    // TODO: What happens if the other event is not found or payload type mismatches?
                    var otherEventDefinition = activityDefinition.InputEventDefinitions[otherEventEntity.Name];
                    var otherEvent = new Event
                    {
                        Id = otherEventEntity.Id,
                        Name = otherEventEntity.Name,
                        DelayDuration = otherEventEntity.DelayDuration,
                        SourceEngineId = otherEventEntity.SourceEngineId,
                    };

                    object payload = this.orchestrator.Engine.Serializer.Deserialize(otherEventEntity.Payload, otherEventDefinition.PayloadType);
                    inputEvents[otherEventEntity.Name] = otherEvent.SetPayload(otherEventDefinition.PayloadType, payload);
                }

                if (inputEvents.Count != activity.AvailableInputEvents.Count)
                {
                    // Some event is missing from event store. This is permanent error and cannot be recovered.
                    var missingEventNames = activity.AvailableInputEvents.Except(inputEvents.Keys);
                    var error = $"Following events are contained in activity but not in event store: {string.Join(",", missingEventNames)}.";
                    throw new WorkflowRuntimeException(isTransient: false, error);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Maps the event message to event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <param name="payloadType">Type of the event payload.</param>
        /// <returns>The mapped event.</returns>
        private Event MapToEvent(EventMessage eventMessage, Type payloadType)
        {
            object payload = payloadType == null
                ? null
                : orchestrator.Engine.Serializer.Deserialize(eventMessage.Payload, payloadType);

            var @event = new Event
            {
                Id = eventMessage.Id,
                Name = eventMessage.EventName,
                DelayDuration = eventMessage.DelayDuration,
                SourceEngineId = eventMessage.SourceEngineId
            };

            return @event.SetPayload(payloadType, payload);
        }

        /// <summary>
        /// Build the activity store key.
        /// </summary>
        /// <param name="workflowName">The workflow name.</param>
        /// <param name="workflowId">The workflow id.</param>
        /// <param name="activityName">The activity name.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The activity store key.</returns>
        private static string GetActivityKey(string workflowName, Guid workflowId, string activityName, string partitionKey)
        {
            return $"{workflowName}/{workflowId}/activities/{activityName}[{partitionKey}]";
        }

        /// <summary>
        /// Build the event store key.
        /// </summary>
        /// <param name="workflowName">The workflow name.</param>
        /// <param name="workflowId">The workflow id.</param>
        /// <param name="activityName">The event name.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The event store key.</returns>
        private static string GetEventKey(string workflowName, Guid workflowId, string eventName, string partitionKey)
        {
            return $"{workflowName}/{workflowId}/events/{eventName}[{partitionKey}]";
        }
    }
}
