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
    using EventDrivenWorkflow.Runtime.Data;

    public static class SimpleRetryWorkflow
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

        private class ExecutableFactory : IExecutableFactory
        {
            private Ref<int> attempRemaining;

            public ExecutableFactory(int attempCount)
            {
                this.attempRemaining = new Ref<int> { Value = attempCount };
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "FailActivity":
                        return new FailActivity(this.attempRemaining);

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
                   ExecutionContext context,
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
