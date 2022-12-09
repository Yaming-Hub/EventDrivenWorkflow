using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Builder;
using EventDrivenWorkflow.Definitions;
using EventDrivenWorkflow.IntegrationTests.Activities;
using EventDrivenWorkflow.IntegrationTests.Environment;
using EventDrivenWorkflow.Runtime;
using EventDrivenWorkflow.Runtime.Data;

namespace EventDrivenWorkflow.IntegrationTests.Workflows
{
    public class InvokeChildWorkflow
    {
        public InvokeChildWorkflow(WorkflowEngine engine)
        {
            // se0 -> b1 -> se1
            var childBuilder = new WorkflowBuilder("Child");
            childBuilder.RegisterEvent("se0");
            childBuilder.RegisterEvent("se1");
            childBuilder.AddActivity("b1").Subscribe("se0").Publish("se1");
            this.ChildWorkflowDefinition = childBuilder.Build();

            this.ChildWorkflowOrchestrator = new WorkflowOrchestrator(
                engine,
                this.ChildWorkflowDefinition,
                new ChildExecutableFactory(this.ChildWorkflowDefinition));

            // e0 -> a1 ... -> e1 -> a2
            var parentBuilder = new WorkflowBuilder("Parent");
            parentBuilder.RegisterEvent("e0");
            parentBuilder.RegisterEvent("e1");
            parentBuilder.AddActivity("a1", isAsync: true).Subscribe("e0").Publish("e1");
            parentBuilder.AddActivity("a2").Subscribe("e1");
            this.ParentWorkflowDefinition = parentBuilder.Build();

            this.ParentWorkflowOrchestrator = new WorkflowOrchestrator(
                engine,
                this.ParentWorkflowDefinition,
                new ParentExecutableFactory(this.ParentWorkflowDefinition, this.ChildWorkflowOrchestrator));
        }

        public WorkflowDefinition ParentWorkflowDefinition { get; }

        public WorkflowDefinition ChildWorkflowDefinition { get; }

        public WorkflowOrchestrator ChildWorkflowOrchestrator { get; }

        public WorkflowOrchestrator ParentWorkflowOrchestrator { get; }

        private class ChildExecutableFactory : IExecutableFactory
        {
            private WorkflowDefinition workflowDefinition;

            public ChildExecutableFactory(
                WorkflowDefinition workflowDefinition)
            {
                this.workflowDefinition = workflowDefinition;
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "b1":
                        return new LogActivity(this.workflowDefinition);
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                return null;
            }
        }

        private class ParentExecutableFactory : IExecutableFactory
        {
            private WorkflowDefinition workflowDefinition;
            private readonly WorkflowOrchestrator childWorkflowOrchestrator;

            public ParentExecutableFactory(
                WorkflowDefinition workflowDefinition,
                WorkflowOrchestrator childWorkflowOrchestrator)
            {
                this.workflowDefinition = workflowDefinition;
                this.childWorkflowOrchestrator = childWorkflowOrchestrator;
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "a2":
                        return new LogActivity(this.workflowDefinition);
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                switch (name)
                {
                    case "a1":
                        return new InvokeChildExecutable(this.childWorkflowOrchestrator);
                }

                return null;
            }

            private class InvokeChildExecutable : IAsyncExecutable
            {
                private readonly WorkflowOrchestrator childWorkflowOrchestrator;

                public InvokeChildExecutable(WorkflowOrchestrator childWorkflowOrchestrator)
                {
                    this.childWorkflowOrchestrator = childWorkflowOrchestrator;
                }

                public async Task BeginExecute(
                   QualifiedExecutionContext context,
                   IEventRetriever eventRetriever)
                {
                    await this.childWorkflowOrchestrator.StartNew(
                        parentExecutionContext: context,
                        eventMap: new Dictionary<string, string>
                        {
                            ["se1"] = "e1"
                        });
                }
            }
        }
    }
}
