// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlMessageHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This message handler handles control messages. It dispatches the control operation to the
    /// corresponding control operation handers.
    /// </summary>
    internal sealed class ControlMessageHandler : IMessageHandler<Message<ControlModel>>
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

        /// <inheritdoc/>
        public async Task<MessageHandleResult> Handle(Message<ControlModel> message)
        {
            if (message.WorkflowExecutionContext.WorkflowName != this.orchestrator.WorkflowDefinition.Name)
            {
                return MessageHandleResult.Continue;
            }

            // Dispatch the message to pre-registered operation handler
            if (!this.operationHandlers.TryGetValue(message.Value.Operation, out var operationHandler))
            {
                // TODO: Report failure as we don't know how to hander this operation.
                return MessageHandleResult.Complete;
            }

            try
            {
                return await operationHandler.Handle(this.orchestrator, message);
            }
            catch (WorkflowRuntimeException wre)
            {
                await this.orchestrator.Engine.Observer.HandleControlMessageFailed(wre, message);
                return wre.IsTransient ? MessageHandleResult.Yield : MessageHandleResult.Complete;
            }
            catch (Exception e)
            {
                await this.orchestrator.Engine.Observer.HandleControlMessageFailed(e, message);
                return MessageHandleResult.Yield; // Unknown exception will be considered as traisent.
            }
        }
    }
}
