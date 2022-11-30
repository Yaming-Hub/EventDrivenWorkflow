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
            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory);

            await orchestrator.StartNew(options: new WorkflowExecutionOptions { TrackProgress = true });

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestCountDownWorkflow()
        {
            var (wd, af) = CountDownWorkflow.Build();
            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew(payload: 3);

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestSimpleAsyncWorkflow()
        {
            var source = new TaskCompletionSource<QualifiedExecutionId>();
            var (wd, af) = SimpleAsyncWorkflow.Build(source);

            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            var qeid = await source.Task;
            await orchestrator.EndExecute(qeid, (c, p) => { p.PublishEvent<string>("result", "the-async-result"); });

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }


        [TestMethod]
        public async Task TestSimpleRetryWorkflow()
        {
            var (wd, af) = SimpleRetryWorkflow.Build(attempCount: 1);

            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestComplexRetryWorkflow()
        {
            var (wd, af) = ComplexRetryWorkflow.Build(attempCount: 2);

            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, wd, af);

            await orchestrator.StartNew();

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }
    }
}