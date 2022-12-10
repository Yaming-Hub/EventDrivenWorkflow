using System.Diagnostics;
using EventDrivenWorkflow;
using EventDrivenWorkflow.Builder;
using EventDrivenWorkflow.Runtime;
using EventDrivenWorkflow.Runtime.IntegrationTests;
using EventDrivenWorkflow.Runtime.Data;
using EventDrivenWorkflow.Memory.Messaging;
using EventDrivenWorkflow.Memory.Persistence;
using EventDrivenWorkflow.Diagnostics;
using EventDrivenWorkflow.IntegrationTests;
using EventDrivenWorkflow.IntegrationTests.Environment;
using EventDrivenWorkflow.Definitions;
using EventDrivenWorkflow.IntegrationTests.Workflows;
using System.Threading.Tasks;

namespace Core.IntegrationTests
{
    [TestClass]
    public class WorkflowTests
    {
        private CancellationTokenSource cancellationTokenSource;
        private TaskCompletionSource taskCompletionSource;
        private WorkflowEngine engine;

        [TestInitialize]
        public void Initialize()
        {
            this.taskCompletionSource = new TaskCompletionSource();

            this.cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            this.cancellationTokenSource.Token.Register(() =>
            {
                this.taskCompletionSource.SetCanceled();
            });

            this.engine = TestWorkflowEngineFactory.CreateMemoryEngine(this.taskCompletionSource);
        }

        [TestMethod]
        public async Task TestSimpleWorkflow()
        {
            var builder = new WorkflowBuilder("Test");
            builder.RegisterEvent("e0");
            builder.RegisterEvent("e1");
            builder.RegisterEvent("e2");
            builder.AddActivity("a1").Subscribe("e0").Publish("e1");
            builder.AddActivity("a2").Subscribe("e1").Publish("e2");
            builder.AddActivity("a3").Subscribe("e2");

            var workflowDefinition = builder.Build();
            var activityFactory = new LogActivityFactory(workflowDefinition);
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory);

            await orchestrator.StartNew(options: new WorkflowExecutionOptions { TrackProgress = true });

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestCountDownWorkflow()
        {
            var (wd, af) = CountDownWorkflow.Build();

            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew(payload: 3);

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestSimpleAsyncWorkflow()
        {
            var source = new TaskCompletionSource<QualifiedActivityExecutionId>();
            var (wd, af) = SimpleAsyncWorkflow.Build(source);

           var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            var qeid = await source.Task;
            await orchestrator.EndExecute(qeid, (c, p) => { p.PublishEvent("result", "the-async-result"); });

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }


        [TestMethod]
        public async Task TestSimpleRetryWorkflow()
        {
            var (wd, af) = SimpleRetryWorkflow.Build(attempCount: 1);
            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            //await taskCompletionSource.Task;
            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestComplexRetryWorkflow()
        {
            var (wd, af) = ComplexRetryWorkflow.Build(attempCount: 2);

            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }
    }
}