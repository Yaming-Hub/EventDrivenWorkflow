// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogActivity.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.IntegrationTests.Activities
{
    using System.Diagnostics;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime.Data;

    public class LogActivity : IExecutable
    {
        private readonly WorkflowDefinition workflowDefinition;

        public LogActivity(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;
        }

        public Task Execute(
            QualifiedExecutionContext context,
            IEventRetriever eventRetriever,
            IEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Execute {context.GetPath()}");

            var activityDefinition = workflowDefinition.ActivityDefinitions[context.ActivityExecutionContext.ActivityName];
            foreach (var outputEvent in activityDefinition.OutputEventDefinitions.Values)
            {
                eventPublisher.PublishEvent(outputEvent.Name);
            }

            return Task.CompletedTask;
        }
    }
}
