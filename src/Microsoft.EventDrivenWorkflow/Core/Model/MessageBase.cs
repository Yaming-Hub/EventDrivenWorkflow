using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class MessageBase
    {
        public Guid Id { get; init; }

        public WorkflowExecutionInfo WorkflowExecutionInfo { get; init; }
    }
}
