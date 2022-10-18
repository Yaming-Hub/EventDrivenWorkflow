using System.Diagnostics;
using Microsoft.EventDrivenWorkflow;
using Microsoft.EventDrivenWorkflow.Builder;
using Microsoft.EventDrivenWorkflow.Runtime;
using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Runtime.Model;
using Microsoft.EventDrivenWorkflow.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Memory.Persistence;

namespace Core.IntegrationTests
{
    [TestClass]
    public class WorkflowTests
    {
        public class LogActivity : IActivity
        {
            public Task Execute(
                ActivityExecutionContext context,
                IEventRetriever eventRetriever,
                IEventPublisher eventPublisher,
                CancellationToken cancellationToken)
            {

                var c = context;
                Trace.WriteLine($"{c.WorkflowName}/{c.WorkflowId}/activities/{c.ActivityName}/{c.ActivityExecutionId}[{c.PartitionKey}]");
                return Task.CompletedTask;
            }
        }

        public class LogActivityFactory : IActivityFactory
        {
            public IActivity CreateActivity(string name)
            {
                return new LogActivity();
            }

            public IAsyncActivity CreateAsyncActivity(string name)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task TestSimpleWorkflow()
        {
            var engine = CreateMemoryEngine();

            var builder = new WorkflowBuilder("Test");
            builder.RegisterEvent("e1");
            builder.RegisterEvent("e2");
            builder.AddActivity("a1").Publish("e1");
            builder.AddActivity("a2").Subscribe("e1").Publish("e2");
            builder.AddActivity("a3").Subscribe("e2");

            var workflowDefinition = builder.Build();

            var activityFactory = new LogActivityFactory();
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory, new WorkflowOrchestrationOptions());

            await orchestrator.StartNew();
        }

        public static WorkflowEngine CreateMemoryEngine()
        {
            var eventStore = new EntityStore<EventEntity>();
            var activityStateStore = new EntityStore<ActivityStateEntity>();
            var eventQueue = new MessageQueue<EventMessage>();
            var controlQueue = new MessageQueue<ControlMessage>();
            var eventMessageProcessor = new MessageProcessor<EventMessage>(eventQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);
            var controlMessageProcessor = new MessageProcessor<ControlMessage>(controlQueue, maxAttemptCount: 2, retryInterval: TimeSpan.Zero);

            var engine = new WorkflowEngine(
                eventMessageProcessor: eventMessageProcessor,
                controlMessageProcessor: controlMessageProcessor,
                eventMessageSender: eventQueue,
                controlMessageSender: controlQueue,
                eventStore: eventStore,
                activityStateStore: activityStateStore,
                serializer: new TestJsonSerializer());

            return engine;
        }

    }
}