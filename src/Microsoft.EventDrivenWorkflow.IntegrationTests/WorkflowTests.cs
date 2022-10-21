using System.Diagnostics;
using Microsoft.EventDrivenWorkflow;
using Microsoft.EventDrivenWorkflow.Builder;
using Microsoft.EventDrivenWorkflow.Runtime;
using Microsoft.EventDrivenWorkflow.Runtime.IntegrationTests;
using Microsoft.EventDrivenWorkflow.Runtime.Data;
using Microsoft.EventDrivenWorkflow.Memory.Messaging;
using Microsoft.EventDrivenWorkflow.Memory.Persistence;
using Microsoft.EventDrivenWorkflow.Diagnostics;
using Microsoft.EventDrivenWorkflow.IntegrationTests;
using Microsoft.EventDrivenWorkflow.IntegrationTests.Environment;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.IntegrationTests.Workflows;
using System.Threading.Tasks;

namespace Core.IntegrationTests
{
    [TestClass]
    public class WorkflowTests
    {
        [TestMethod]
        public async Task TestSimpleWorkflow()
        {
            var builder = new WorkflowBuilder("Test", WorkflowType.Static);
            builder.RegisterEvent("e1");
            builder.RegisterEvent("e2");
            builder.AddActivity("a1").Publish("e1");
            builder.AddActivity("a2").Subscribe("e1").Publish("e2");
            builder.AddActivity("a3").Subscribe("e2");

            var workflowDefinition = builder.Build();
            var activityFactory = new LogActivityFactory(workflowDefinition);
            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory);

            await orchestrator.StartNew();

            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }

        [TestMethod]
        public async Task TestCountDownWorkflow()
        {
            var (wd, af) = CountDownWorkflow.Build();
            ;
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
    }
}