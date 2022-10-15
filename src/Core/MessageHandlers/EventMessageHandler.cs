using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Contract.Persistence;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.MessageHandlers
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
            catch (Exception e)
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

            var eventDefinition = workflowDefinition.EventDefinitions.FirstOrDefault(ed => ed.Name == message.EventName);
            if (eventDefinition == null)
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

            // Find the activity that subscribe to the current event. Please note, there should be exactly
            // one activity listen to the event.
            var activityDefinition = workflowDefinition.ActivityDefinitions
                .FirstOrDefault(a => a.InputEventDefinitions.Any(e => e.Name == message.EventName));
            if (activityDefinition == null)
            {
                // There is no activity subscribe to the event
                if (workflowDefinition.Type == WorkflowType.Close)
                {
                    // This may happen if the workflow is changed.
                    // TODO: Report unsubscribed event error.
                    return MessageHandleResult.Complete;
                }
                else // WorkflowType.Open
                {
                    // TODO: Add the event into output event list of the open workflow.
                    throw new NotImplementedException();
                }
            }

            var evt = MapToEvent(message, eventDefinition.PayloadType);
            var wei = message.WorkflowExecutionInfo;
            var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count)
            {
                [message.EventName] = evt
            };

            if (activityDefinition.InputEventDefinitions.Count > 1)
            {
                // If the activity subscribes to multiple events, then it cannot be triggered by a single event.
                // To solve this problem, an extra activity state is being introduced to track the input event
                // availability. The events are also persisted in an external storage. Try to load other input 
                // events and update activity states to see if the activity is ready to execute.
                var allInputEventsAvailable = await TryLoadInputEvents(workflowDefinition, activityDefinition, wei, evt, inputEvents);
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
            WorkflowDefinition workflowDefinition,
            ActivityDefinition activityDefinition,
            WorkflowExecutionInfo wei,
            Event evt,
            Dictionary<string, Event> inputEvents)
        {

            var activityKey = GetActivityKey(wei.WorkflowName, wei.WorkflowId, activityDefinition.Name, wei.PartitionKey);
            var eventKey = GetEventKey(wei.WorkflowName, wei.WorkflowId, evt.Name, wei.PartitionKey);

            var activity = await orchestrator.Engine.ActivityStore.GetOrAdd(
                wei.PartitionKey,
                activityKey,
                () => new ActivityStateEntity
                {
                    WorkflowName = wei.WorkflowName,
                    WorkflowVersion = wei.WorkflowVersion,
                    WorkflowId = wei.WorkflowId,
                    Name = activityDefinition.Name,
                    AvailableInputEvents = new List<string> { evt.Name }
                });

            // Persist event to store. The event may have already been persisted in case of recurrent activity, here always
            // override existing event.
            await orchestrator.Engine.EventStore.Upsert(wei.PartitionKey, eventKey, evt);

            if (!activity.AvailableInputEvents.Contains(evt.Name))
            {
                // The event is not in the list, that means the activity is fetched from store. Add the event to
                // activity and try to persist it.
                activity.AvailableInputEvents.Add(evt.Name);

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
                    if (!activity.AvailableInputEvents.Contains(evt.Name))
                    {
                        activity.AvailableInputEvents.Add(evt.Name);
                    }

                    await orchestrator.Engine.ActivityStore.Update(wei.PartitionKey, activityKey, activity);
                }
            }

            if (activity.AvailableInputEvents.Count == activityDefinition.InputEventDefinitions.Count)
            {
                var otherEventKeys = activity.AvailableInputEvents
                    .Where(n => n != evt.Name)
                    .Select(n => GetEventKey(wei.WorkflowName, wei.WorkflowId, n, wei.PartitionKey))
                    .ToList();

                var otherEvents = await orchestrator.Engine.EventStore.GetMany(wei.PartitionKey, otherEventKeys);

                foreach (var otherEvent in otherEvents)
                {
                    inputEvents[otherEvent.Name] = otherEvent;
                }

                if (inputEvents.Count != activity.AvailableInputEvents.Count)
                {
                    // Some event is missing from event store. This is permanent error and cannot be recovered.
                    var missingEventNames = activity.AvailableInputEvents.Except(inputEvents.Keys);
                    var message = $"Following events are contained in activity but not in event store: {string.Join(",", missingEventNames)}.";
                    throw new WorkflowException(isTransient: false, message);
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
            object payload = payloadType == typeof(void)
                ? null
                : orchestrator.Engine.Serializer.Deserialize(eventMessage.Payload, payloadType);

            return new Event
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
