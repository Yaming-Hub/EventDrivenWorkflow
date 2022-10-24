// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestWorkflowEngineFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    using Microsoft.EventDrivenWorkflow.Memory.Messaging;
    using Microsoft.EventDrivenWorkflow.Memory.Persistence;
    using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;
    using Microsoft.EventDrivenWorkflow.Runtime;

    public class TestWorkflowEngineFactory
    {
        public static WorkflowEngine CreateMemoryEngine()
        {
            var eventStore = new EntityStore<Entity<EventModel>>();
            var activityStateStore = new EntityStore<Entity<ActivityState>>();
            var activityExecutionContextStore = new EntityStore<Entity<ExecutionContext>>();
            var eventPresenceStore = new EntityStore<Entity<EventReference>>();
            var eventQueue = new MessageQueue<Message<EventModel>>();
            var controlQueue = new MessageQueue<Message<ControlModel>>();
            var eventMessageProcessor = new MessageProcessor<Message<EventModel>>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<Message<ControlModel>>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            eventQueue.AddProcessor(eventMessageProcessor);
            controlQueue.AddProcessor(controlMessageProcessor);

            var engine = new WorkflowEngine(
                id: "test",
                eventMessageProcessor: eventMessageProcessor,
                controlMessageProcessor: controlMessageProcessor,
                eventMessageSender: eventQueue,
                controlMessageSender: controlQueue,
                eventStore: eventStore,
                activityStateStore: activityStateStore,
                eventPresenceStore: eventPresenceStore,
                activityExecutionContextStore: activityExecutionContextStore,
                serializer: new TestJsonSerializer(),
                observer: new TraceWorkflowObserver());

            return engine;
        }
    }
}
