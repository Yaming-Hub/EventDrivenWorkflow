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
    public class SubWorkflowTests
    {
        [TestMethod]
        public async Task TestSubWorkflow()
        {
            // e0 -> a1 ... se0 -> b1 -> se1  ... e1 -> a2

            var builder = new WorkflowBuilder("Parent");
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
    }
}