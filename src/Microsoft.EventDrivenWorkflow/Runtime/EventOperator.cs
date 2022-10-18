// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventOperator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.Model;

    /// <summary>
    /// This class defines the event operator of an activity.
    /// </summary>
    public sealed class EventOperator : IEventPublisher, IEventRetriever
    {
        /// <summary>
        /// A dictionary contains input events.
        /// </summary>
        private readonly IReadOnlyDictionary<string, EventData> inputEvents;

        /// <summary>
        /// A dictionary contains output events.
        /// </summary>
        private readonly Dictionary<string, EventData> outputEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventOperator"/> class.
        /// </summary>
        /// <param name="activityDefinition">The execiting activity definition.</param>
        /// <param name="activityExecutionContext">The activity execution information.</param>
        /// <param name="inputEvents">A dictionary contains input events of the execiting activity.</param>
        internal EventOperator(
            ActivityDefinition activityDefinition,
            ActivityExecutionContext activityExecutionContext,
            IReadOnlyDictionary<string, EventData> inputEvents)
        {
            this.ActivityDefinition = activityDefinition;
            this.ActivityExecutionInfo = activityExecutionContext;

            this.inputEvents = inputEvents;
            this.outputEvents = new Dictionary<string, EventData>();
        }

        /// <summary>
        /// Gets the executing activity definition.
        /// </summary>
        public ActivityDefinition ActivityDefinition { get; }

        /// <summary>
        /// Gets activity executing execution info.
        /// </summary>
        public ActivityExecutionContext ActivityExecutionInfo { get; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void PublishEvents(params Event[] events)
        {
            if (this.ActivityDefinition.IsAsync)
            {
                throw new InvalidOperationException("Events can only published for synchronized activity.");
            }

            this.PublishEventInternal(events);
        }

        /// <summary>
        /// Publish events.
        /// </summary>
        /// <param name="events">The outupt events.</param>
        /// <exception cref="ArgumentException">thrown if output events are not valid.</exception>
        /// <exception cref="InvalidOperationException">thrown if the one of output events is already published.</exception>
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

        /// <summary>
        /// Gets output events.
        /// </summary>
        /// <returns>A list of output events.</returns>
        internal IEnumerable<EventData> GetOutputEvents() => this.outputEvents.Values;

        /// <summary>
        /// Validate input events.
        /// </summary>
        internal void ValidateInputEvents()
        {
            // TODO: Check if input events matches activity definition.
        }

        /// <summary>
        /// Validate output events.
        /// </summary>
        internal void ValidateOutputEvents()
        {
            // TODO: Check if the output events matches activity definition.
        }
    }
}
