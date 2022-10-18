using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    internal interface IControlOperationHandler
    {
        Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, ControlMessage message);
    }
}
