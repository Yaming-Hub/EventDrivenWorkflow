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
    using EventDrivenWorkflow.Diagnostics;

    public class TestWorkflowEngineFactory
    {
        public static WorkflowEngine CreateMemoryEngine(TaskCompletionSource taskCompletionSource = null)
        {
            var eventStore = new EntityStore<Entity<EventModel>>();
            var activityStateStore = new EntityStore<Entity<ActivityState>>();
            var activityExecutionContextStore = new EntityStore<Entity<QualifiedExecutionContext>>();
            var eventPresenceStore = new EntityStore<Entity<EventReference>>();
            var eventQueue = new MessageQueue<EventMessage>();
            var controlQueue = new MessageQueue<ControlMessage>();
            var eventMessageProcessor = new MessageProcessor<EventMessage>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<ControlMessage>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            eventQueue.AddProcessor(eventMessageProcessor);
            controlQueue.AddProcessor(controlMessageProcessor);

            var observer = new PipelineWorkflowObserver(
                new TraceWorkflowObserver(),
                new CompletenessWorkflowObserver(taskCompletionSource)
            );

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
                observer: observer);

            return engine;
        }
    }
}
