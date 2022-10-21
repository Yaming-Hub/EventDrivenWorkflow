using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Runtime.Data;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class LogActivity : IExecutable
    {
        private readonly WorkflowDefinition workflowDefinition;

        public LogActivity(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;
        }

        public Task Execute(
            ActivityExecutionContext context,
            IEventRetriever eventRetriever,
            IEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Execute {context.GetPath()}");

            var activityDefinition = this.workflowDefinition.ActivityDefinitions[context.ActivityName];
            foreach (var outputEvent in activityDefinition.OutputEventDefinitions.Values)
            {
                eventPublisher.PublishEvent(outputEvent.Name);
            }

            return Task.CompletedTask;
        }
    }
}
