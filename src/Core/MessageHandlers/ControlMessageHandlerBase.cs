using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.MessageHandlers
{
    public abstract class ControlMessageHandlerBase : IMessageHandler<ControlMessage>
    {
        public ControlMessageHandlerBase(WorkflowOrchestrator orchestrator)
        {
            this.Orchestrator = orchestrator;
        }

        protected WorkflowOrchestrator Orchestrator { get; }

        protected abstract ControlOperation Operation { get; }

        public async Task<MessageHandleResult> Handle(ControlMessage message)
        {
            if (message.WorkflowExecutionInfo.WorkflowName != this.Orchestrator.WorkflowDefinition.Name)
            {
                return MessageHandleResult.Continue;
            }

            if (message.Operation != this.Operation)
            {
                return MessageHandleResult.Continue;
            }

            try
            {
                return await HandleInternal(message);
            }
            catch (WorkflowException we)
            {
                // TODO: Track exception
                return we.IsTransient ? MessageHandleResult.Yield : MessageHandleResult.Complete;
            }
            catch
            {
                // TODO: Track exception

                // Unknown exception will be considered as traisent.
                return MessageHandleResult.Yield;
            }
        }

        protected abstract Task<MessageHandleResult> HandleInternal(ControlMessage message);
    }
}
