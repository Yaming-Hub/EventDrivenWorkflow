using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Persistence;
using Microsoft.EventDrivenWorkflow.Runtime.Data;
using Microsoft.EventDrivenWorkflow.Utilities;

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    internal sealed class InputEventLoader
    {
        /// <summary>
        /// The workflow ochestrator.
        /// </summary>
        private readonly WorkflowOrchestrator orchestrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventLoader"/> class.
        /// </summary>
        /// <param name="orchestrator">The workflow ochestrator.</param>
        public InputEventLoader(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        public async Task<bool> TryLoadInputEvents(
            ActivityDefinition activityDefinition,
            WorkflowExecutionContext workflowExecutionContext,
            EventModel triggerEventModel,
            Dictionary<string, Event> inputEvents)
        {
            if (activityDefinition.InputEventDefinitions.Count == 0)
            {
                throw new WorkflowRuntimeException(isTransient: false, $"Activity {activityDefinition.Name} does not have any input event.");
            }

            if (!activityDefinition.InputEventDefinitions.TryGetValue(triggerEventModel.Name, out var triggerEventDefinition))
            {
                throw new WorkflowRuntimeException(
                    isTransient: false, $"Activity {activityDefinition.Name} does not have any input event {triggerEventModel.Name}.");
            }

            var triggerEvent = MapToEvent(triggerEventModel, triggerEventDefinition.PayloadType);
            inputEvents[triggerEvent.Name] = triggerEvent;

            if (activityDefinition.InputEventDefinitions.Count == 1)
            {
                // Activity with single input event does not need to maintain activity state and events in store.
                return true;
            }

            // For activities with more than one input event, given handler receive one event at one time, so
            // the events need to be persisted in a store, also use activity state to track the which input events
            // are available.
            var activityKey = ResourceKeyFormat.GetActivityKey(
                partitionKey: workflowExecutionContext.PartitionKey,
                workflowName: workflowExecutionContext.WorkflowName,
                workflowId: workflowExecutionContext.WorkflowId,
                activityName: activityDefinition.Name);

            var eventKey = ResourceKeyFormat.GetEventKey(
                partitionKey: workflowExecutionContext.PartitionKey,
                workflowName: workflowExecutionContext.WorkflowName,
                workflowId: workflowExecutionContext.WorkflowId,
                eventName: triggerEvent.Name);

            // The activity state tracks which inputs are available.
            var activityStateEntity = await orchestrator.Engine.ActivityStateStore.GetOrAdd(
                workflowExecutionContext.PartitionKey,
                activityKey,
                () => new Entity<ActivityState>
                {
                    Value = new ActivityState
                    {
                        WorkflowName = workflowExecutionContext.WorkflowName,
                        WorkflowVersion = workflowExecutionContext.WorkflowVersion,
                        WorkflowId = workflowExecutionContext.WorkflowId,
                        Name = activityDefinition.Name,
                        AvailableInputEvents = new List<string> { triggerEvent.Name }
                    },
                    ExpireDateTime = workflowExecutionContext.WorkflowExpireDateTime,
                });

            // Persist the trigger event to store. The event may have already been persisted in case of recurrent activity, here always
            // override existing event.
            var eventEntity = new Entity<EventModel>
            {
                Value = triggerEventModel,
                ExpireDateTime = workflowExecutionContext.WorkflowExpireDateTime
            };

            // TODO: This step is not required in case of retry execution.
            await orchestrator.Engine.EventStore.Upsert(workflowExecutionContext.PartitionKey, eventKey, eventEntity);

            if (!activityStateEntity.Value.AvailableInputEvents.Contains(triggerEvent.Name))
            {
                // The event is not in the list, that means the activity is fetched from store. Add the event to
                // activity and try to persist it.
                activityStateEntity.Value.AvailableInputEvents.Add(triggerEvent.Name);

                try
                {
                    await orchestrator.Engine.ActivityStateStore.Update(workflowExecutionContext.PartitionKey, activityKey, activityStateEntity);
                }
                catch (StoreException se) when (se.ErrorCode == StoreErrorCode.EtagMismatch)
                {
                    // The activity has been updated by another orchestrator. Re-load the activity and try again.
                    // Please note, the code will only try one more time here, if the update operation continue to 
                    // fail, then let the orchestrator to handle it.
                    activityStateEntity = await orchestrator.Engine.ActivityStateStore.Get(workflowExecutionContext.PartitionKey, activityKey);
                    if (!activityStateEntity.Value.AvailableInputEvents.Contains(triggerEvent.Name))
                    {
                        activityStateEntity.Value.AvailableInputEvents.Add(triggerEvent.Name);
                    }

                    await orchestrator.Engine.ActivityStateStore.Update(workflowExecutionContext.PartitionKey, activityKey, activityStateEntity);
                }
            }

            if (activityStateEntity.Value.AvailableInputEvents.Count == activityDefinition.InputEventDefinitions.Count)
            {
                var otherEventKeys = activityStateEntity.Value.AvailableInputEvents
                    .Where(n => n != triggerEvent.Name)
                    .Select(n => ResourceKeyFormat.GetEventKey(
                        partitionKey: workflowExecutionContext.PartitionKey,
                        workflowName: workflowExecutionContext.WorkflowName,
                        workflowId: workflowExecutionContext.WorkflowId,
                        eventName: n))
                    .ToList();

                var otherEventEntities = await orchestrator.Engine.EventStore.GetMany(workflowExecutionContext.PartitionKey, otherEventKeys);

                foreach (var otherEventEntity in otherEventEntities)
                {
                    // TODO: What happens if the other event is not found or payload type mismatches?
                    var otherEventDefinition = activityDefinition.InputEventDefinitions[otherEventEntity.Value.Name];
                    var otherEvent = new Event
                    {
                        Id = otherEventEntity.Value.Id,
                        Name = otherEventEntity.Value.Name,
                        DelayDuration = otherEventEntity.Value.DelayDuration,
                        SourceEngineId = otherEventEntity.Value.SourceEngineId,
                    };

                    object payload = orchestrator.Engine.Serializer.Deserialize(otherEventEntity.Value.Payload.Body, otherEventDefinition.PayloadType);
                    inputEvents[otherEventEntity.Value.Name] = otherEvent.SetPayload(otherEventDefinition.PayloadType, payload);
                }

                if (inputEvents.Count != activityStateEntity.Value.AvailableInputEvents.Count)
                {
                    // Some event is missing from event store. This is permanent error and cannot be recovered.
                    var missingEventNames = activityStateEntity.Value.AvailableInputEvents.Except(inputEvents.Keys);
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
        /// <param name="eventModel">The event message.</param>
        /// <param name="payloadType">Type of the event payload.</param>
        /// <returns>The mapped event.</returns>
        private Event MapToEvent(EventModel eventModel, Type payloadType)
        {
            object payload = payloadType == null
                ? null
                : orchestrator.Engine.Serializer.Deserialize(eventModel.Payload.Body, payloadType);

            var @event = new Event
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                DelayDuration = eventModel.DelayDuration,
                SourceEngineId = eventModel.SourceEngineId,
            };

            return @event.SetPayload(payloadType, payload);
        }
    }
}
