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
using Microsoft.EventDrivenWorkflow.IntegrationTests.Environment;

namespace Core.IntegrationTests
{
    [TestClass]
    public class WorkflowTests
    {
        [TestMethod]
        public async Task TestSimpleWorkflow()
        {
            var builder = new WorkflowBuilder("Test");
            builder.RegisterEvent("e1");
            builder.RegisterEvent("e2");
            builder.AddActivity("a1").Publish("e1");
            builder.AddActivity("a2").Subscribe("e1").Publish("e2");
            builder.AddActivity("a3").Subscribe("e2");

            var workflowDefinition = builder.Build();

            var activityFactory = new LogActivityFactory();
            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var orchestrator = new WorkflowOrchestrator(engine, workflowDefinition, activityFactory, new WorkflowOrchestrationOptions());

            await orchestrator.StartNew();

            await Task.Delay(TimeSpan.FromSeconds(1));

            Trace.WriteLine("Done");
        }
    }
}