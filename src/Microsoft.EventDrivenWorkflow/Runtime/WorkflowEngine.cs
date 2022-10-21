﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowEngine.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using Microsoft.EventDrivenWorkflow.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Persistence;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

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
            IMessageProcessor<Message<EventModel>> eventMessageProcessor,
            IMessageProcessor<Message<ControlModel>> controlMessageProcessor,
            IMessageSender<Message<EventModel>> eventMessageSender,
            IMessageSender<Message<ControlModel>> controlMessageSender,
            ISerializer serializer,
            IEntityStore<Entity<EventModel>> eventStore,
            IEntityStore<Entity<ActivityState>> activityStateStore,
            IEntityStore<Entity<ActivityExecutionContext>> activityExecutionContextStore,
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
        internal IMessageProcessor<Message<EventModel>> EventMessageProcessor { get; }

        /// <summary>
        /// Gets the control message processor.
        /// </summary>
        internal IMessageProcessor<Message<ControlModel>> ControlMessageProcessor { get; }

        /// <summary>
        /// Gets event message sender.
        /// </summary>
        internal IMessageSender<Message<EventModel>> EventMessageSender { get; }

        /// <summary>
        /// Gets control message sender.
        /// </summary>
        internal IMessageSender<Message<ControlModel>> ControlMessageSender { get; }

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
        internal IEntityStore<Entity<ActivityExecutionContext>> ActivityExecutionContextStore { get; }

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
