using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    /// <summary>
    /// This message handler handles control messages. It dispatches the control operation to the
    /// corresponding control operation handers.
    /// </summary>
    public sealed class ControlMessageHandler : IMessageHandler<ControlMessage>
    {
        private readonly WorkflowOrchestrator orchestrator;
        private readonly IReadOnlyDictionary<ControlOperation, IControlOperationHandler> operationHandlers;

        public ControlMessageHandler(WorkflowOrchestrator orchestrator)
            : this(orchestrator, operationHanders: null)
        {

        }

        internal ControlMessageHandler(
            WorkflowOrchestrator orchestrator,
            IReadOnlyDictionary<ControlOperation, IControlOperationHandler> operationHanders)
        {
            this.orchestrator = orchestrator;
            this.operationHandlers = operationHandlers ?? new Dictionary<ControlOperation, IControlOperationHandler>
            {
                [ControlOperation.ExecuteActivity] = new ExecuteActivityOperationHandler(),
            };
        }


        public async Task<MessageHandleResult> Handle(ControlMessage message)
        {
            if (message.WorkflowExecutionContext.WorkflowName != this.orchestrator.WorkflowDefinition.Name)
            {
                return MessageHandleResult.Continue;
            }

            if (!this.operationHandlers.TryGetValue(message.Operation, out var operationHandler))
            {
                // TODO: Report failure as we don't know how to hander this operation.
                return MessageHandleResult.Complete;
            }

            try
            {
                return await operationHandler.Handle(this.orchestrator, message);
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
    }
}
