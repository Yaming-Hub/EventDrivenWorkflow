// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogActivity.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    using System.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    public class LogActivity : IExecutable
    {
        private readonly WorkflowDefinition workflowDefinition;

        public LogActivity(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;
        }

        public Task Execute(
            ExecutionContext context,
            IEventRetriever eventRetriever,
            IEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Execute {context.GetPath()}");

            var activityDefinition = this.workflowDefinition.ActivityDefinitions[context.ActivityExecutionContext.ActivityName];
            foreach (var outputEvent in activityDefinition.OutputEventDefinitions.Values)
            {
                eventPublisher.PublishEvent(outputEvent.Name);
            }

            return Task.CompletedTask;
        }
    }
}
