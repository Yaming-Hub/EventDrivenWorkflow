using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Messaging;

namespace Microsoft.EventDrivenWorkflow.Core
{
    public class WorkflowControlMessageHandler : IMessageHandler<ControlMessage>
    {
        private WorkflowOrchestrator orchestrator;

        public WorkflowControlMessageHandler(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        public Task<MessageHandleResult> Handle(ControlMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
