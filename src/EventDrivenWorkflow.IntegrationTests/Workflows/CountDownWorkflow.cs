﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountDownWorkflow.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Workflows
{
    using System.Diagnostics;
    using EventDrivenWorkflow.Builder;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime.Data;

    public static class CountDownWorkflow
    {
        public static (WorkflowDefinition, IExecutableFactory) Build()
        {
            var builder = new WorkflowBuilder("CountDown");
            builder.RegisterEvent<int>("countParameter");
            builder.RegisterEvent<int>("countVarible");
            builder.AddActivity("ForwardActivity").Subscribe("countParameter").Publish("countVarible");
            builder.AddActivity("CountDownActivity").Subscribe("countVarible").Publish("countVarible");

            return (builder.Build(), new ExecutableFactory());
        }

        private class ExecutableFactory : IExecutableFactory
        {
            public IExecutable CreateExecutable(string name)
            {
                switch (name)
                {
                    case "ForwardActivity":
                        return new Forward();

                    case "CountDownActivity":
                        return new CountDown();
                }

                return null;
            }

            public IAsyncExecutable CreateAsyncExecutable(string name)
            {
                throw new NotImplementedException();
            }

            private class Forward : IExecutable
            {
                public Task Execute(
                   QualifiedExecutionContext context,
                   IEventRetriever eventRetriever,
                   IEventPublisher eventPublisher,
                   CancellationToken cancellationToken)
                {
                    int count = (int)eventRetriever.GetEvent("countParameter").Value;
                    Trace.WriteLine($"[ForwardActivity] Count={count} Path={context.GetPath()}");
                    eventPublisher.PublishEvent("countVarible", count);
                    return Task.CompletedTask;
                }
            }

            private class CountDown : IExecutable
            {
                public Task Execute(
                    QualifiedExecutionContext context,
                    IEventRetriever eventRetriever,
                    IEventPublisher eventPublisher,
                    CancellationToken cancellationToken)
                {
                    int count = (int)eventRetriever.GetEvent("countVarible").Value;
                    Trace.WriteLine($"[CountDownActivity] Count={count} Path={context.GetPath()}");

                    count--;
                    if (count > 0)
                    {
                        eventPublisher.PublishEvent("countVarible", count);
                    }

                    return Task.CompletedTask;
                }
            }
        }
    }
}
