using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class ActivityExecutionContext : IActivityExecutionContext
    {
        private readonly IReadOnlyDictionary<string, Event> inputEvents;

        private readonly Dictionary<string, Event> outputEvents;

        public ActivityExecutionContext(
            WorkflowDefinition workflowDefinition,
            ActivityDefinition activityDefinition,
            WorkflowExecutionInfo workflowExecutionInfo,
            Guid activityExecutionId,
            IReadOnlyDictionary<string, Event> inputEvents)
        {
            this.WorkflowDefinition = workflowDefinition;
            this.ActivityDefinition = activityDefinition;
            this.WorkflowExecutionInfo = workflowExecutionInfo;
            this.ActivityExecutionId = activityExecutionId;

            this.inputEvents = inputEvents;
            this.outputEvents = new Dictionary<string, Event>();
        }

        public WorkflowDefinition WorkflowDefinition { get; }

        public ActivityDefinition ActivityDefinition { get; }

        public WorkflowExecutionInfo WorkflowExecutionInfo { get; }

        public Guid ActivityExecutionId { get; }

        public T GetEventPayload<T>(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("The event name cannot be null or empty string.", nameof(eventName));
            }

            if (!this.ActivityDefinition.InputEventDefinitions.Any(e => e.Name == eventName))
            {
                throw new ArgumentException($"The event name {eventName} is not defined.");
            }

            if (!this.inputEvents.TryGetValue(eventName, out var evt))
            {
                throw new InvalidOperationException($"The input event {eventName} is not found.");
            }

            return (T)evt.Payload;
        }

        public void PublishEvent(string eventName)
        {
            this.PublishEvent(eventName, delay: TimeSpan.Zero);
        }

        public void PublishEvent<T>(string eventName, T payload)
        {
            this.PublishEvent(eventName, payload, delay: TimeSpan.Zero);
        }

        public void PublishEvent(string eventName, TimeSpan delay)
        {
            this.outputEvents[eventName] = new Event { Name = eventName, Delay = delay };
        }

        public void PublishEvent<T>(string eventName, T payload, TimeSpan delay)
        {
            this.outputEvents[eventName] = new Event { Name = eventName, Payload = payload, Delay = delay };
        }

        internal IEnumerable<Event> GetOutputEvents() => this.outputEvents.Values;

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
