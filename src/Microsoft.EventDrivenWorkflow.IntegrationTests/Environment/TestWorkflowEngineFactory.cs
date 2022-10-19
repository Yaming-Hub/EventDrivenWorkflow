using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Memory.Persistence;
using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Runtime.Model;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class TestWorkflowEngineFactory
    {
        public static WorkflowEngine CreateMemoryEngine()
        {
            var eventStore = new EntityStore<EventEntity>();
            var activityStateStore = new EntityStore<ActivityStateEntity>();
            var eventQueue = new MessageQueue<EventMessage>();
            var controlQueue = new MessageQueue<ControlMessage>();
            var eventMessageProcessor = new MessageProcessor<EventMessage>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<ControlMessage>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
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
                serializer: new TestJsonSerializer(),
                observer: new TraceWorkflowObserver());

            return engine;
        }
    }
}
