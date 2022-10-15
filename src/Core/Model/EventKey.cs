using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class EventKey
    {
        public string WorkflowName { get; init; }

        public string EventName { get; init; }

        public string PartitionKey { get; init; }

        public Guid WorkflowExecutionId { get; init; }
    }
}
