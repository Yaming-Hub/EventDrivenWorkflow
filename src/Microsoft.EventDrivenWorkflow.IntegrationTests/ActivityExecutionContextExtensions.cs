﻿

namespace Microsoft.EventDrivenWorkflow.IntegrationTests
{

    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    public static class ActivityExecutionContextExtensions
    {
        public static string GetPath(this ExecutionContext context)
        {
            return context.QualifiedExecutionId.ToString();
        }

        public static string GetPath(this WorkflowExecutionContext context)
        {
            var c = context;
            return $"{c.WorkflowName}/{c.WorkflowId}[{c.PartitionKey}]";
        }
    }
}
