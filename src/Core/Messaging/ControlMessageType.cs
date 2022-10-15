using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Messaging
{
    public enum ControlMessageType
    {
        ExecuteActivity,
        WorkflowTimeout,
        MultiInputExecuteCheck,
        Error
    }
}
