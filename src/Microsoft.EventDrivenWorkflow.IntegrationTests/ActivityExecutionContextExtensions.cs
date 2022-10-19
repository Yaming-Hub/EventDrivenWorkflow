using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests
{
    public static class ActivityExecutionContextExtensions
    {
        public static string GetPath(this ActivityExecutionContext context)
        {
            var c = context;
            return $"{c.WorkflowName}/{c.WorkflowId}/activities/{c.ActivityName}/{c.ActivityExecutionId}[{c.PartitionKey}]";
        }

        public static string GetPath(this WorkflowExecutionContext context)
        {
            var c = context;
            return $"{c.WorkflowName}/{c.WorkflowId}[{c.PartitionKey}]";
        }
    }
}
