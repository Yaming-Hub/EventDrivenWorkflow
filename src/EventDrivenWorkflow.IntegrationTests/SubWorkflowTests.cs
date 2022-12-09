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
            var engine = TestWorkflowEngineFactory.CreateMemoryEngine();
            var workflow = new InvokeChildWorkflow(engine);

            await workflow.ParentWorkflowOrchestrator.StartNew();


            await Task.Delay(TimeSpan.FromSeconds(3));

            Trace.WriteLine("Done");
        }
    }
}