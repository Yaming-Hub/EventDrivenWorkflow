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

    public static class SimpleAsyncWorkflow
    {
        public static (WorkflowDefinition, IExecutableFactory) Build(TaskCompletionSource<QualifiedActivityExecutionId> taskCompletionSource)
        {
            var builder = new WorkflowBuilder("SimpleAsyncWorkflow");
            builder.RegisterEvent("e0");
            builder.RegisterEvent<string>("result");
            builder.AddActivity("AsyncActivity", isAsync: true).Subscribe("e0").Publish("result");
            builder.AddActivity("LogResult").Subscribe("result");
            return (builder.Build(), new ExecutableFactory(taskCompletionSource));
        }

        private class ExecutableFactory : IExecutableFactory
        {
            private readonly TaskCompletionSource<QualifiedActivityExecutionId> taskCompletionSource;

            public ExecutableFactory(TaskCompletionSource<QualifiedActivityExecutionId> taskCompletionSource)
            {
                this.taskCompletionSource = taskCompletionSource;
            }

            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "LogResult":
                        return new LogResult<string>();
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                switch (name)
                {
                    case "AsyncActivity":
                        return new AsyncExecutable(this.taskCompletionSource);
                }

                return null;
            }

            private class AsyncExecutable : IAsyncExecutable
            {
                private readonly TaskCompletionSource<QualifiedActivityExecutionId> taskCompletionSource;

                public AsyncExecutable(TaskCompletionSource<QualifiedActivityExecutionId> taskCompletionSource)
                {
                    this.taskCompletionSource = taskCompletionSource;
                }

                public Task BeginExecute(
                   QualifiedExecutionContext context,
                   IEventRetriever eventRetriever)
                {
                    Trace.WriteLine($"[AsyncExecutable.BeginExecute] Path={context.GetPath()}");
                    this.taskCompletionSource.SetResult(context.ActivityExecutionId);

                    return Task.CompletedTask;
                }
            }
        }
    }
}
