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
        public async Task TestSequentialWorkflow()
        {
            var workflow = new SequentialWorkflow(engine: this.engine);

            await workflow.Orchestrator.StartNew(options: new WorkflowExecutionOptions { TrackProgress = true });

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestCountDownWorkflow()
        {
            var workflow = new CountDownWorkflow(engine: this.engine);

            await workflow.Orchestrator.StartNew(payload: 3);

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
            var workflow = new SimpleRetryWorkflow(engine: this.engine, attemptCount: 3);

            await workflow.Orchestrator.StartNew();

            await taskCompletionSource.Task;
            //await Task.Delay(TimeSpan.FromSeconds(3));

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