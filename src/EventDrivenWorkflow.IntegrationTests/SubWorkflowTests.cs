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
        public async Task TestSubWorkflow()
        {
            var workflow = new InvokeChildWorkflow(engine);

            await workflow.ParentWorkflowOrchestrator.StartNew();

            await taskCompletionSource.Task;

            Trace.WriteLine("Done");
        }
    }
}