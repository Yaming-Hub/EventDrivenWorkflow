﻿// --------------------------------------------------------------------------------------------------------------------
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
        public static (WorkflowDefinition, IExecutableFactory) Build(TaskCompletionSource<QualifiedExecutionId> taskCompletionSource)
        {
            var builder = new WorkflowBuilder("SimpleAsyncWorkflow", WorkflowType.Static);
            builder.RegisterEvent<string>("result");
            builder.AddActivity("AsyncActivity", isAsync: true).Publish("result");
            builder.AddActivity("LogResult").Subscribe("result");
            return (builder.Build(), new ExecutableFactory(taskCompletionSource));
        }

        private class ExecutableFactory : IExecutableFactory
        {
            private readonly TaskCompletionSource<QualifiedExecutionId> taskCompletionSource;

            public ExecutableFactory(TaskCompletionSource<QualifiedExecutionId> taskCompletionSource)
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
                private readonly TaskCompletionSource<QualifiedExecutionId> taskCompletionSource;

                public AsyncExecutable(TaskCompletionSource<QualifiedExecutionId> taskCompletionSource)
                {
                    this.taskCompletionSource = taskCompletionSource;
                }

                public Task BeginExecute(
                   ExecutionContext context,
                   IEventRetriever eventRetriever)
                {
                    Trace.WriteLine($"[AsyncExecutable.BeginExecute] Path={context.GetPath()}");
                    this.taskCompletionSource.SetResult(context.QualifiedExecutionId);

                    return Task.CompletedTask;
                }
            }
        }
    }
}