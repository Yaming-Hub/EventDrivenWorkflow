using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.Messaging
{
    public class MessageBase
    {
        public WorkflowExecutionInfo WorkflowExecutionInfo { get; init; }
    }
}
