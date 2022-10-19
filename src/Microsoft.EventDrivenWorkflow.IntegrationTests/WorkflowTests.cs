using System.Diagnostics;
using Microsoft.EventDrivenWorkflow;
using Microsoft.EventDrivenWorkflow.Builder;
using Microsoft.EventDrivenWorkflow.Runtime;
using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Runtime.Model;
using Microsoft.EventDrivenWorkflow.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Memory.Persistence;
using Microsoft.EventDrivenWorkflow.Diagnostics;
using Microsoft.EventDrivenWorkflow.IntegrationTests;

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
                Trace.WriteLine("Execute context.GetExecutionPath()");
                return Task.CompletedTask;
            }
        }

        public class TraceWorkflowObserver : IWorkflowObserver
        {
            public Task WorkflowStarted(WorkflowExecutionContext context)
            {
                return Log($"WorkflowStarted    Workflow={context.GetPath()}");
            }

            public Task EventAccepted(WorkflowExecutionContext context, Event @event)
            {
                return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
            }

            public Task ActivityStarting(ActivityExecutionContext context, IEnumerable<Event> inputEvents)
            {
                return Log($"ActivityStarting   Activity={context.GetPath()} Events={string.Join(",", inputEvents.Select(x => x.Name))}");
            }

            public Task ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents)
            {
                return Log($"ActivityCompleted  Activity={context.GetPath()} Events={string.Join(",", outputEvents.Select(x => x.Name))}");
            }

            public Task EventPublished(WorkflowExecutionContext context, Event @event)
            {
                return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
            }

            public Task WorkflowCompleted(WorkflowExecutionContext context)
            {
                return Log($"WorkflowCompleted  Workflow={context.GetPath()}");
            }

            private static Task Log(string text)
            {
                Trace.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")} {text}");
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

            await Task.Delay(TimeSpan.FromSeconds(1));

            Trace.WriteLine("Done");
        }

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