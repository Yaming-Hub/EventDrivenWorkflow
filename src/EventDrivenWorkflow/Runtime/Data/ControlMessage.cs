// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines a message wrapper.
    /// </summary>
    /// <typeparam name="T">Type of the message body.</typeparam>
    public sealed class ControlMessage : WorkflowMessage
    {
        /// <summary>
        /// Gets the message body value.
        /// </summary>
        public ControlModel ControlModel { get; init; }

        /// <summary>
        /// Gets the control operation.
        /// </summary>
        public ControlOperation Operation { get; init; }
    }
}
