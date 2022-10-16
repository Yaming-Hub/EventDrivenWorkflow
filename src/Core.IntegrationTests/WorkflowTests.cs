using Microsoft.EventDrivenWorkflow.Core;
using Microsoft.EventDrivenWorkflow.Core.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Core.Model;
using Microsoft.EventDrivenWorkflow.Provider.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Provider.Memory.Persistence;

namespace Core.IntegrationTests
{
    [TestClass]
    public class WorkflowTests
    {
        [TestMethod]
        public Task TestSimpleWorkflow()
        {
            var engine = CreateMemoryEngine();

            return Task.CompletedTask;
        }

        public static WorkflowEngine CreateMemoryEngine()
        {
            var eventStore = new EntityStore<EventEntity>();
            var activityStore = new EntityStore<ActivityStateEntity>();
            var eventQueue = new MessageQueue<EventMessage>();
            var controlQueue = new MessageQueue<ControlMessage>();
            var eventMessageProcessor = new MessageProcessor<EventMessage>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<ControlMessage>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);

            var engine = new WorkflowEngine(
                eventMessageProcessor: eventMessageProcessor,
                controlMessageProcessor: controlMessageProcessor,
                eventMessageSender: eventQueue,
                controlMessageSender: controlQueue,
                serializer: new TestJsonSerializer(),
                eventStore: eventStore,
                activityStore: activityStore);

            return engine;
        }

    }
}