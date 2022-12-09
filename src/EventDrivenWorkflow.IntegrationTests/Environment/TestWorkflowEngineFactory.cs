// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestWorkflowEngineFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    using EventDrivenWorkflow.Memory.Messaging;
    using EventDrivenWorkflow.Memory.Persistence;
    using EventDrivenWorkflow.Runtime.IntegrationTests;
    using EventDrivenWorkflow.Runtime.Data;
    using EventDrivenWorkflow.Runtime;

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

            var workflowOrchestratorProvider = new TestWorkflowOrchestratorProvider(); 
            var eventMessageProcessor = new MessageProcessor<Message<EventModel>>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<Message<ControlModel>>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            eventQueue.AddProcessor(eventMessageProcessor);
            controlQueue.AddProcessor(controlMessageProcessor);

            var engine = new WorkflowEngine(
                id: "test",
                workflowOrchestratorProvider: workflowOrchestratorProvider,
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
