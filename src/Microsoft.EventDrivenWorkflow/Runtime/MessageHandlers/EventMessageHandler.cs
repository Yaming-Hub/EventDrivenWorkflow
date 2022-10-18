using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Persistence;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
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
            catch (WorkflowException we)
            {
                // TODO: Track exception
                return we.IsTransient ? MessageHandleResult.Yield : MessageHandleResult.Complete;
            }
            catch
            {
                // TODO: Track exception

                // Unknown exception will be considered as traisent.
                return MessageHandleResult.Yield;
            }
        }

        private async Task<MessageHandleResult> HandleInternal(EventMessage message)
        {
            var workflowDefinition = orchestrator.WorkflowDefinition;
            if (message.WorkflowExecutionInfo.WorkflowName != workflowDefinition.Name)
            {
                // The event doesn't belong to this workflow, it should be handled by another handler.
                return MessageHandleResult.Continue; // Ignore if the event doesn't belong to this workflow.
            }

            if (!workflowDefinition.EventDefinitions.TryGetValue(message.EventName, out var eventDefinition))
            {
                // Got an unknown event. This may happen if the workflow is changed.
                // TODO: Report unknown event error.
                return MessageHandleResult.Complete;
            }

            if (eventDefinition.PayloadType.FullName != message.PayloadType)
            {
                // The incoming event payload type doesn't match the event definition, the workflow logic may have been changed.
                // TODO: Report invalid event error.
                return MessageHandleResult.Complete;
            }

            // Find the activity that subscribe to the current event. Please note, there should be no more than one activity
            // subscribe to the same event in the workflow.
            if (!workflowDefinition.EventToSubscribedActivityMap.TryGetValue(message.EventName, out var activityDefinition))
            {
                // This may happen if the workflow is changed.
                // TODO: Report unsubscribed event error.
                return MessageHandleResult.Complete;
            }

            var eventData = MapToEventData(message, eventDefinition.PayloadType);
            var wei = message.WorkflowExecutionInfo;
            var inputEvents = new Dictionary<string, EventData>(capacity: activityDefinition.InputEventDefinitions.Count)
            {
                [message.EventName] = eventData
            };

            if (activityDefinition.InputEventDefinitions.Count > 1)
            {
                // If the activity subscribes to multiple events, then it cannot be triggered by a single event.
                // To solve this problem, an extra activity state is being introduced to track the input event
                // availability. The events are also persisted in an external storage. Try to load other input 
                // events and update activity states to see if the activity is ready to execute.
                var allInputEventsAvailable = await TryLoadInputEvents(
                    activityDefinition,
                    wei,
                    message,
                    eventData,
                    inputEvents);

                if (!allInputEventsAvailable)
                {
                    // The activity is not yet ready to execute.
                    return MessageHandleResult.Complete;
                }
            }

            // Execute the activity when all inputs are ready.
            await this.orchestrator.ActivityExecutor.Execute(
                workflowDefinition,
                activityDefinition,
                wei,
                inputEvents);

            return MessageHandleResult.Complete;
        }

        private async Task<bool> TryLoadInputEvents(
            ActivityDefinition activityDefinition,
            WorkflowExecutionInfo wei,
            EventMessage message,
            EventData eventData,
            Dictionary<string, EventData> inputEvents)
        {
            var activityKey = GetActivityKey(wei.WorkflowName, wei.WorkflowId, activityDefinition.Name, wei.PartitionKey);
            var eventKey = GetEventKey(wei.WorkflowName, wei.WorkflowId, eventData.Name, wei.PartitionKey);

            var activity = await orchestrator.Engine.ActivityStore.GetOrAdd(
                wei.PartitionKey,
                activityKey,
                () => new ActivityStateEntity
                {
                    WorkflowName = wei.WorkflowName,
                    WorkflowVersion = wei.WorkflowVersion,
                    WorkflowId = wei.WorkflowId,
                    Name = activityDefinition.Name,
                    AvailableInputEvents = new List<string> { eventData.Name }
                });

            // Persist event to store. The event may have already been persisted in case of recurrent activity, here always
            // override existing event.
            var eventEntity = new EventEntity
            {
                Id = eventData.Id,
                Name = eventData.Name,
                DelayDuration = eventData.DelayDuration,
                PayloadType = message.PayloadType,
                Payload = message.Payload,
            };

            await orchestrator.Engine.EventStore.Upsert(wei.PartitionKey, eventKey, eventEntity);

            if (!activity.AvailableInputEvents.Contains(eventData.Name))
            {
                // The event is not in the list, that means the activity is fetched from store. Add the event to
                // activity and try to persist it.
                activity.AvailableInputEvents.Add(eventData.Name);

                try
                {
                    await orchestrator.Engine.ActivityStore.Update(wei.PartitionKey, activityKey, activity);
                }
                catch (StoreException se) when (se.ErrorCode == StoreErrorCode.EtagMismatch)
                {
                    // The activity has been updated by another orchestrator. Re-load the activity and try again.
                    // Please note, the code will only try one more time here, if the update operation continue to 
                    // fail, then let the orchestrator to handle it.
                    activity = await orchestrator.Engine.ActivityStore.Get(wei.PartitionKey, activityKey);
                    if (!activity.AvailableInputEvents.Contains(eventData.Name))
                    {
                        activity.AvailableInputEvents.Add(eventData.Name);
                    }

                    await orchestrator.Engine.ActivityStore.Update(wei.PartitionKey, activityKey, activity);
                }
            }

            if (activity.AvailableInputEvents.Count == activityDefinition.InputEventDefinitions.Count)
            {
                var otherEventKeys = activity.AvailableInputEvents
                    .Where(n => n != eventData.Name)
                    .Select(n => GetEventKey(wei.WorkflowName, wei.WorkflowId, n, wei.PartitionKey))
                    .ToList();

                var otherEventEntities = await orchestrator.Engine.EventStore.GetMany(wei.PartitionKey, otherEventKeys);

                foreach (var otherEventEntity in otherEventEntities)
                {
                    // TODO: What happens if the other event is not found or payload type mismatches?
                    var otherEventDefinition = activityDefinition.InputEventDefinitions[otherEventEntity.Name];
                    inputEvents[otherEventEntity.Name] = new EventData
                    {
                        Id = otherEventEntity.Id,
                        Name = otherEventEntity.Name,
                        DelayDuration = otherEventEntity.DelayDuration,
                        Payload = this.orchestrator.Engine.Serializer.Deserialize(otherEventEntity.Payload, otherEventDefinition.PayloadType),
                    };
                }

                if (inputEvents.Count != activity.AvailableInputEvents.Count)
                {
                    // Some event is missing from event store. This is permanent error and cannot be recovered.
                    var missingEventNames = activity.AvailableInputEvents.Except(inputEvents.Keys);
                    var error = $"Following events are contained in activity but not in event store: {string.Join(",", missingEventNames)}.";
                    throw new WorkflowException(isTransient: false, error);
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
        private EventData MapToEventData(EventMessage eventMessage, Type payloadType)
        {
            object payload = payloadType == null
                ? null
                : orchestrator.Engine.Serializer.Deserialize(eventMessage.Payload, payloadType);

            return new EventData
            {
                Id = eventMessage.Id,
                Name = eventMessage.EventName,
                Payload = payload,
            };
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
