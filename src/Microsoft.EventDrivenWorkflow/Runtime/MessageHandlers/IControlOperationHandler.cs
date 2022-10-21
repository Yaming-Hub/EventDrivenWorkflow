﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IControlOperationHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers
{
    using Microsoft.EventDrivenWorkflow.Messaging;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This interface defines a control operation handler.
    /// </summary>
    internal interface IControlOperationHandler
    {
        /// <summary>
        /// Handle the specific control message.
        /// </summary>
        /// <param name="orchestrator">The workflow orchestrator.</param>
        /// <param name="message">The control message.</param>
        /// <returns>The handle result.</returns>
        Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, Message<ControlModel> message);
    }
}
