using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Runtime.Model
{
    public class MessageBase
    {
        public Guid Id { get; init; }

        public WorkflowExecutionContext WorkflowExecutionContext { get; init; }
    }
}
