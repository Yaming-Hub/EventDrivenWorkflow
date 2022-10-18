using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class ActivityExecutionContext : IActivityExecutionContext
    {
        private readonly IReadOnlyDictionary<string, EventData> inputEvents;

        private readonly Dictionary<string, EventData> outputEvents;

        public ActivityExecutionContext(
            WorkflowDefinition workflowDefinition,
            ActivityDefinition activityDefinition,
            ActivityExecutionInfo activityExecutionInfo,
            IReadOnlyDictionary<string, EventData> inputEvents)
        {
            this.WorkflowDefinition = workflowDefinition;
            this.ActivityDefinition = activityDefinition;
            this.ActivityExecutionInfo = activityExecutionInfo;

            this.inputEvents = inputEvents;
            this.outputEvents = new Dictionary<string, EventData>();
        }

        public WorkflowDefinition WorkflowDefinition { get; }

        public ActivityDefinition ActivityDefinition { get; }

        public ActivityExecutionInfo ActivityExecutionInfo { get; }

        public T GetInputEventPayload<T>(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("The event name cannot be null or empty string.", nameof(eventName));
            }

            if (!this.ActivityDefinition.InputEventDefinitions.TryGetValue(eventName, out var eventDefinition))
            {
                throw new ArgumentException($"The input event {eventName} is not defined.");
            }

            if (eventDefinition.PayloadType != typeof(T))
            {
                throw new ArgumentException(
                    $"The input event {eventName} payload type {eventDefinition.PayloadType.FullName} " +
                    $"is different from the requesting payload type {typeof(T).FullName}.");
            }

            if (!this.inputEvents.TryGetValue(eventName, out var eventData))
            {
                throw new InvalidOperationException($"The input event {eventName} is not found.");
            }

            return (T)eventData.Payload;
        }

        public void PublishEvent(params Event[] events)
        {
            if (this.ActivityDefinition.IsAsync)
            {
                throw new InvalidOperationException("Events can only published for synchronized activity.");
            }

            this.PublishEventInternal(events);
        }

        internal void PublishEventInternal(params Event[] events)
        {
            // Do not add to output events directly so in case there is any invalid event
            // the output events will remain unchanged.
            var eventDataList = new List<EventData>(events.Length);
            foreach (var @event in events)
            {
                if (string.IsNullOrEmpty(@event.Name))
                {
                    throw new ArgumentException("Event must have name");
                }

                if (!this.ActivityDefinition.OutputEventDefinitions.TryGetValue(@event.Name, out var eventDefinition))
                {
                    throw new ArgumentException($"The output event name {@event.Name} is not defined.");
                }

                if (this.outputEvents.ContainsKey(@event.Name))
                {
                    throw new InvalidOperationException($"The output event {@event.Name} is already published.");
                }

                object payload = null;
                if (eventDefinition.PayloadType != null)
                {
                    try
                    {
                        payload = @event.GetPayload(eventDefinition.PayloadType);
                    }
                    catch (InvalidCastException ice)
                    {
                        throw new ArgumentException($"The event {@event.Name} must have payload with type {eventDefinition.PayloadType.FullName}.", ice);
                    }
                }

                eventDataList.Add(new EventData
                {
                    Name = @event.Name,
                    Payload = payload,
                    DelayDuration = @event.DelayDuration,
                });
            }

            foreach (var coreEvent in eventDataList)
            {
                this.outputEvents.Add(coreEvent.Name, coreEvent);
            }
        }

        internal IEnumerable<EventData> GetOutputEvents() => this.outputEvents.Values;

        internal void ValidateInputEvents()
        {
            // TODO (ymliu): Check if input events matches activity definition.
        }

        internal void ValidateOutputEvents()
        {
            // TODO (ymliu): Check if the output events matches activity definition.
        }
    }
}
