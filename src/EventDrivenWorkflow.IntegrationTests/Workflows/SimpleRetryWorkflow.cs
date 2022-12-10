// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleAsyncWorkflow.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Workflows
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EventDrivenWorkflow.Builder;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.IntegrationTests.Activities;
    using EventDrivenWorkflow.Runtime;
    using EventDrivenWorkflow.Runtime.Data;

    public class SimpleRetryWorkflow
    {
        public static (WorkflowDefinition, IExecutableFactory) Build(int attempCount)
        {
            var builder = new WorkflowBuilder("RetryWorkflow");
            builder.RegisterEvent("e0");
            builder.RegisterEvent<string>("result");
            builder.AddActivity("FailActivity").Subscribe("e0").Publish("result").Retry(maxRetryCount: 4, delayDuration: TimeSpan.Zero);
            builder.AddActivity("LogResult").Subscribe("result");
            return (builder.Build(), new ExecutableFactory(attempCount));
        }

        public SimpleRetryWorkflow(WorkflowEngine engine, int attemptCount = 3)
        {
            var builder = new WorkflowBuilder("RetryWorkflow");
            builder.RegisterEvent("e0");
            builder.RegisterEvent<string>("result");
            builder.AddActivity("FailActivity").Subscribe("e0").Publish("result").Retry(maxRetryCount: 4, delayDuration: TimeSpan.Zero);
            builder.AddActivity("LogResult").Subscribe("result");

            this.Definition = builder.Build();
            this.Orchestrator = new WorkflowOrchestrator(
                engine,
                this.Definition,
                new ExecutableFactory(attemptCount));
        }

        public WorkflowDefinition Definition { get; }

        public WorkflowOrchestrator Orchestrator { get; }


        private class ExecutableFactory : IExecutableFactory
        {
            private Ref<int> attemptRemaining;

            public ExecutableFactory(int attemptCount)
            {
                this.attemptRemaining = new Ref<int> { Value = attemptCount };
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "FailActivity":
                        return new FailActivity(this.attemptRemaining);

                    case "LogResult":
                        return new LogResult<string>();
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                throw new NotImplementedException();
            }

            private class FailActivity : IExecutable
            {
                private Ref<int> attempRemaining;

                public FailActivity(Ref<int> attempRemaining)
                {
                    this.attempRemaining = attempRemaining;
                }

                public Task Execute(
                   QualifiedExecutionContext context,
                   IEventRetriever eventRetriever,
                   IEventPublisher eventPublisher,
                   CancellationToken cancellationToken)
                {
                    if (this.attempRemaining.Value > 0)
                    {
                        this.attempRemaining.Value--;
                        Trace.WriteLine($"[FailActivity.Execute] Throw Path={context.GetPath()}");

                        throw new Exception($"Fail at iteration {this.attempRemaining.Value}");
                    }

                    eventPublisher.PublishEvent("result", "done");
                    Trace.WriteLine($"[FailActivity.Execute] Done Path={context.GetPath()}");
                    return Task.CompletedTask;
                }
            }

        }
    }
}
