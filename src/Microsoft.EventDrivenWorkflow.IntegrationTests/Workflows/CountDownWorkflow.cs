using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Builder;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.IntegrationTests.Environment;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Workflows
{
    public static class CountDownWorkflow
    {
        public static (WorkflowDefinition, IActivityFactory) Build()
        {
            var builder = new WorkflowBuilder("CountDown", WorkflowType.Dynamic);
            builder.RegisterEvent<int>("countParameter");
            builder.RegisterEvent<int>("countVarible");
            builder.AddActivity("ForwardActivity").Subscribe("countParameter").Publish("countVarible");
            builder.AddActivity("CountDownActivity").Subscribe("countVarible").Publish("countVarible");
            var wd = builder.Build();

            return (builder.Build(), new ActivityFactory());
        }

        private class ActivityFactory : IActivityFactory
        {
            public IActivity CreateActivity(string name)
            {
                switch (name)
                {
                    case "ForwardActivity":
                        return new ForwardActivity();

                    case "CountDownActivity":
                        return new CountDownActivity();
                }

                return null;
            }

            public IAsyncActivity CreateAsyncActivity(string name)
            {
                throw new NotImplementedException();
            }

            private class ForwardActivity : IActivity
            {
                public Task Execute(
                   ActivityExecutionContext context,
                   IEventRetriever eventRetriever,
                   IEventPublisher eventPublisher,
                   CancellationToken cancellationToken)
                {
                    int count = eventRetriever.GetEvent<int>("countParameter").Payload;
                    Trace.WriteLine($"[ForwardActivity] Count={count} Path={context.GetPath()}");
                    eventPublisher.PublishEvent("countVarible", count);
                    return Task.CompletedTask;
                }
            }

            private class CountDownActivity : IActivity
            {
                public Task Execute(
                    ActivityExecutionContext context,
                    IEventRetriever eventRetriever,
                    IEventPublisher eventPublisher,
                    CancellationToken cancellationToken)
                {
                    int count = eventRetriever.GetEvent<int>("countVarible").Payload;
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
