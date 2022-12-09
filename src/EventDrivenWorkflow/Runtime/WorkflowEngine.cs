// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowEngine.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    using EventDrivenWorkflow.Diagnostics;
    using EventDrivenWorkflow.Messaging;
    using EventDrivenWorkflow.Persistence;
    using EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This class defines a workflow engine.
    /// </summary>
    public sealed class WorkflowEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowEngine"/> class.
        /// </summary>
        /// <param name="id">The engine id.</param>
        /// <param name="eventMessageProcessor">The event message processor.</param>
        /// <param name="controlMessageProcessor">The control message processor.</param>
        /// <param name="eventMessageSender">The event message sender.</param>
        /// <param name="controlMessageSender">The control message sender.</param>
        /// <param name="eventStore">The event store.</param>
        /// <param name="activityStateStore">The activity state store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="timeProvider">The time provider.</param>
        public WorkflowEngine(
            string id,
            IMessageProcessor<EventMessage> eventMessageProcessor,
            IMessageProcessor<ControlMessage> controlMessageProcessor,
            IMessageSender<EventMessage> eventMessageSender,
            IMessageSender<ControlMessage> controlMessageSender,
            ISerializer serializer,
            IEntityStore<Entity<EventModel>> eventStore,
            IEntityStore<Entity<ActivityState>> activityStateStore,
            IEntityStore<Entity<QualifiedExecutionContext>> activityExecutionContextStore,
            IEntityStore<Entity<EventReference>> eventPresenceStore,
            IWorkflowObserver observer,
            ITimeProvider timeProvider = null)
        {
            this.Id = id;
            this.EventMessageProcessor = eventMessageProcessor;
            this.ControlMessageProcessor = controlMessageProcessor;
            this.EventMessageSender = eventMessageSender;
            this.ControlMessageSender = controlMessageSender;
            this.EventStore = eventStore;
            this.ActivityStateStore = activityStateStore;
            this.ActivityExecutionContextStore = activityExecutionContextStore;
            this.EventPresenseStore = eventPresenceStore;
            this.Serializer = serializer;
            this.Observer = new SafeWorkflowObserver(observer);
            this.TimeProvider = timeProvider ?? new DefaultTimeProvider();
        }

        /// <summary>
        /// Gets engine id.
        /// </summary>
        internal string Id { get; }

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
        internal IEntityStore<Entity<EventModel>> EventStore { get; }

        /// <summary>
        /// Gets the activity state store.
        /// </summary>
        internal IEntityStore<Entity<ActivityState>> ActivityStateStore { get; }

        /// <summary>
        /// Gets the activity execution context store.
        /// </summary>
        internal IEntityStore<Entity<QualifiedExecutionContext>> ActivityExecutionContextStore { get; }

        /// <summary>
        /// Gets the event presence store.
        /// </summary>
        internal IEntityStore<Entity<EventReference>> EventPresenseStore { get; }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        internal ISerializer Serializer { get; }

        /// <summary>
        /// Gets the workflow observer.
        /// </summary>
        internal IWorkflowObserver Observer { get; }

        /// <summary>
        /// Gets time provider.
        /// </summary>
        internal ITimeProvider TimeProvider { get; }
    }
}
