using EventDrivenWorkflow.Builder;
using EventDrivenWorkflow.Definitions;
using EventDrivenWorkflow.IntegrationTests.Activities;
using EventDrivenWorkflow.Runtime;

namespace EventDrivenWorkflow.IntegrationTests.Workflows
{
    public class SequentialWorkflow
    {
        public SequentialWorkflow(WorkflowEngine engine)
        {
            // e0 -> a1 -> e1 -> a2 -> e2 -> a3
            var builder = new WorkflowBuilder("Test");
            builder.RegisterEvent("e0");
            builder.RegisterEvent("e1");
            builder.RegisterEvent("e2");
            builder.AddActivity("a1").Subscribe("e0").Publish("e1");
            builder.AddActivity("a2").Subscribe("e1").Publish("e2");
            builder.AddActivity("a3").Subscribe("e2");

            this.Definition = builder.Build();
            this.Orchestrator = new WorkflowOrchestrator(
                engine,
                this.Definition,
                new ExecutableFactory(this.Definition));
        }

        public WorkflowDefinition Definition { get; }

        public WorkflowOrchestrator Orchestrator { get; }

        private class ExecutableFactory : IExecutableFactory
        {
            private WorkflowDefinition workflowDefinition;

            public ExecutableFactory(
                WorkflowDefinition workflowDefinition)
            {
                this.workflowDefinition = workflowDefinition;
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "a1":
                    case "a2":
                    case "a3":
                        return new LogActivity(this.workflowDefinition);
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                return null;
            }
        }
    }
}
