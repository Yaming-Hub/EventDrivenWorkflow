using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    /// <summary>
    /// This class defines the fully qualified activity execution id.
    /// </summary>
    public sealed class ActivityExecutionIdentity
    {
        public string PartitionKey { get; init; }

        public string WorkflowName { get; init; }

        public string WorkflowVersion { get; init; }

        public Guid WorkflowId { get; init; }

        public string ActivityName { get; init; }

        public Guid ActivityExecutionId { get; init; }
    }
}
