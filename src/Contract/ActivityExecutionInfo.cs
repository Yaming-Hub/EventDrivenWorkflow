using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public sealed class ActivityExecutionInfo : WorkflowExecutionInfo
    {
        public string ActivityName { get; init; }

        public Guid ActivityExecutionId { get; init; }

        public DateTime ActivityStartDateTime { get; init; }

        public ActivityExecutionIdentity GetActivityExecutionIdentity() =>
            new ActivityExecutionIdentity
            {
                PartitionKey = this.PartitionKey,
                WorkflowName = this.WorkflowName,
                WorkflowIdentity = this.WorkflowId,
                ActivityName = this.ActivityName,
                ActivityExecutionId = this.ActivityExecutionId
            };
    }
}
