using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Memory.Persistence;
using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Runtime.Data;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class TestWorkflowEngineFactory
    {
        public static WorkflowEngine CreateMemoryEngine()
        {
            var eventStore = new EntityStore<Entity<EventModel>>();
            var activityStateStore = new EntityStore<Entity<ActivityState>>();
            var activityExecutionContextStore = new EntityStore<Entity<ActivityExecutionContext>>();
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
                activityExecutionContextStore: activityExecutionContextStore,
                serializer: new TestJsonSerializer(),
                observer: new TraceWorkflowObserver());

            return engine;
        }
    }
}
