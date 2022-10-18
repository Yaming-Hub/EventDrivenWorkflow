// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowEngine.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Persistence;
    using Microsoft.EventDrivenWorkflow.Runtime.Model;

    /// <summary>
    /// This class defines a workflow engine.
    /// </summary>
    public sealed class WorkflowEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowEngine"/> class.
        /// </summary>
        /// <param name="eventMessageProcessor">The event message processor.</param>
        /// <param name="controlMessageProcessor">The control message processor.</param>
        /// <param name="eventMessageSender">The event message sender.</param>
        /// <param name="controlMessageSender">The control message sender.</param>
        /// <param name="eventStore">The event store.</param>
        /// <param name="activityStateStore">The activity state store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="timeProvider">The time provider.</param>
        public WorkflowEngine(
            IMessageProcessor<EventMessage> eventMessageProcessor,
            IMessageProcessor<ControlMessage> controlMessageProcessor,
            IMessageSender<EventMessage> eventMessageSender,
            IMessageSender<ControlMessage> controlMessageSender,
            ISerializer serializer,
            IEntityStore<EventEntity> eventStore,
            IEntityStore<ActivityStateEntity> activityStateStore,
            ITimeProvider timeProvider = null)
        {
            this.EventMessageProcessor = eventMessageProcessor;
            this.ControlMessageProcessor = controlMessageProcessor;
            this.EventMessageSender = eventMessageSender;
            this.ControlMessageSender = controlMessageSender;
            this.EventStore = eventStore;
            this.ActivityStateStore = activityStateStore;
            this.Serializer = serializer;
            this.TimeProvider = timeProvider ?? new DefaultTimeProvider();
        }

        /// <summary>
        /// Gets the event message processor.
        /// </summary>
        internal IMessageProcessor<EventMessage> EventMessageProcessor { get; }

        /// <summary>
        /// Gets the control message processor.
        /// </summary>
        internal IMessageProcessor<ControlMessage> ControlMessageProcessor { get; }

        /// <summary>
        /// Gets event message sender.
        /// </summary>
        internal IMessageSender<EventMessage> EventMessageSender { get; }

        /// <summary>
        /// Gets control message sender.
        /// </summary>
        internal IMessageSender<ControlMessage> ControlMessageSender { get; }

        /// <summary>
        /// Gets the event store.
        /// </summary>
        internal IEntityStore<EventEntity> EventStore { get; }

        /// <summary>
        /// Gets the activity state store.
        /// </summary>
        internal IEntityStore<ActivityStateEntity> ActivityStateStore { get; }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        internal ISerializer Serializer { get; }

        /// <summary>
        /// Gets time provider.
        /// </summary>
        internal ITimeProvider TimeProvider { get; }
    }
}
