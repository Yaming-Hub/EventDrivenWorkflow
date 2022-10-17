using System.Diagnostics;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Builder;
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
        public class LogActivity : IActivity
        {
            public Task Execute(IActivityExecutionContext context, CancellationToken cancellationToken)
            {
                var aei = context.ActivityExecutionInfo;
                Trace.WriteLine($"{aei.WorkflowName}/{aei.WorkflowId}/activities/{aei.ActivityName}/{aei.ActivityExecutionId}[{aei.PartitionKey}]");
                return Task.CompletedTask;
            }
        }

        public class LogActivityFactory : IActivityFactory
        {
            public IActivity CreateActivity(string partitionKey, string name)
            {
                return new LogActivity();
            }

            public IAsyncActivity CreateAsyncActivity(string partitionKey, string name)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public Task TestSimpleWorkflow()
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
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory, new WorkflowOptions());

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