// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountDownWorkflow.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Workflows
{
    using System.Diagnostics;
    using EventDrivenWorkflow.Builder;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime;
    using EventDrivenWorkflow.Runtime.Data;

    public class CountDownWorkflow
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

        public CountDownWorkflow(WorkflowEngine engine)
        {
            // countParameter -> Forward -> countVariable -> CountDown -> countVariable -> ... -> CountDown
            var builder = new WorkflowBuilder("CountDown");
            builder.RegisterEvent<int>("countParameter");
            builder.RegisterEvent<int>("countVarible");
            builder.AddActivity("ForwardActivity").Subscribe("countParameter").Publish("countVarible");
            builder.AddActivity("CountDownActivity").Subscribe("countVarible").Publish("countVarible");

            this.Definition = builder.Build();
            this.Orchestrator = new WorkflowOrchestrator(
                engine,
                this.Definition,
                new ExecutableFactory());
        }

        public WorkflowDefinition Definition { get; }

        public WorkflowOrchestrator Orchestrator { get; }

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
                    int count = eventRetriever.GetEventValue<int>("countParameter");
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
                    int count = eventRetriever.GetEventValue<int>("countVarible");
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
