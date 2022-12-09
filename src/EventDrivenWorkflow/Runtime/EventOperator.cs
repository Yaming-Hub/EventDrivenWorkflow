// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventOperator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This class defines the event operator of an activity.
    /// </summary>
    public sealed class EventOperator : IEventPublisher, IEventRetriever
    {
        /// <summary>
        /// Gets workflow orchestrator.
        /// </summary>
        private readonly WorkflowOrchestrator orchestrator;

        /// <summary>
        /// A dictionary contains input events.
        /// </summary>
        private readonly IReadOnlyDictionary<string, Event> inputEvents;

        /// <summary>
        /// A dictionary contains output events.
        /// </summary>
        private readonly Dictionary<string, Event> outputEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventOperator"/> class.
        /// </summary>
        /// <param name="activityDefinition">The execiting activity definition.</param>
        /// <param name="context">The activity execution information.</param>
        /// <param name="inputEvents">A dictionary contains input events of the execiting activity.</param>
        internal EventOperator(
            WorkflowOrchestrator orchestrator,
            ActivityDefinition activityDefinition,
            ExecutionContext context,
            IReadOnlyDictionary<string, Event> inputEvents)
        {
            this.orchestrator = orchestrator;
            this.ActivityDefinition = activityDefinition;
            this.Context = context;

            this.inputEvents = inputEvents;
            this.outputEvents = new Dictionary<string, Event>();
        }

        /// <summary>
        /// Gets the executing activity definition.
        /// </summary>
        public ActivityDefinition ActivityDefinition { get; }

        /// <summary>
        /// Gets activity executing execution info.
        /// </summary>
        public ExecutionContext Context { get; }

        public Event GetEvent(string eventName)
        {
            return this.GetEvent(eventName, payloadType: null);
        }

        ///// <inheritdoc/>
        //public Event<T> GetEvent<T>(string eventName)
        //{
        //    return (Event<T>)this.GetEvent(eventName, typeof(T));
        //}

        /// <inheritdoc/>
        public void PublishEvent(string eventName)
        {
            this.PublishEvent(eventName, delayDuration: TimeSpan.Zero);
        }

        /// <inheritdoc/>
        public void PublishEvent(string eventName, TimeSpan delayDuration)
        {
            this.ValidateOutputEvent(eventName, payloadType: null, payload: null);

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Name = eventName,
                DelayDuration = delayDuration,
                SourceEngineId = this.orchestrator.Engine.Id
            };

            this.outputEvents.Add(eventName, @event);
        }

        /// <inheritdoc/>
        public void PublishEvent(string eventName, object payload)
        {

            this.PublishEvent(eventName, payload, delayDuration: TimeSpan.Zero);
        }


        /// <inheritdoc/>
        public void PublishEvent(string eventName, object payload, TimeSpan delayDuration)
        {
            this.ValidateOutputEvent(eventName, payloadType: payload?.GetType(), payload: payload);

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Name = eventName,
                DelayDuration = delayDuration,
                Payload = payload,
                SourceEngineId = this.orchestrator.Engine.Id
            };

            this.outputEvents.Add(eventName, @event);
        }

        /// <summary>
        /// Gets event by name.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="payloadType">The payload type.</param>
        /// <returns>The event object.</returns>
        private Event GetEvent(string eventName, Type payloadType)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("The event name cannot be null or empty string.", nameof(eventName));
            }

            if (!this.ActivityDefinition.InputEventDefinitions.TryGetValue(eventName, out var eventDefinition))
            {
                throw new ArgumentException($"The input event {eventName} is not defined.");
            }

            if (eventDefinition.PayloadType != payloadType)
            {
                throw new ArgumentException(
                    $"The input event {eventName} payload type {eventDefinition.PayloadType?.FullName ?? "<null>"} " +
                    $"is different from the requesting payload type {payloadType?.FullName ?? "<null>"}.");
            }

            if (!this.inputEvents.TryGetValue(eventName, out Event @event))
            {
                throw new InvalidOperationException($"The input event {eventName} is not found.");
            }

            return @event;
        }

        private void ValidateOutputEvent(string eventName, Type payloadType, object payload)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("Event must have name");
            }

            if (!this.ActivityDefinition.OutputEventDefinitions.TryGetValue(eventName, out var eventDefinition))
            {
                throw new ArgumentException($"The output event name {eventName} is not defined.");
            }

            if (eventDefinition.PayloadType != payloadType)
            {
                throw new ArgumentException(
                    $"The output event {eventName} payload type {eventDefinition.PayloadType?.FullName ?? "<null>"} " +
                    $"is different from the parameter payload type {payloadType?.FullName ?? "<null>"}.");
            }

            if (this.outputEvents.ContainsKey(eventName))
            {
                throw new InvalidOperationException($"The output event {eventName} is already published.");
            }
        }

        /// <summary>
        /// Gets input events.
        /// </summary>
        /// <returns>A list of input events.</returns>
        internal IEnumerable<Event> GetInputEvents() => this.inputEvents.Values;

        /// <summary>
        /// Gets output events.
        /// </summary>
        /// <returns>A list of output events.</returns>
        internal IEnumerable<Event> GetOutputEvents() => this.outputEvents.Values;

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
