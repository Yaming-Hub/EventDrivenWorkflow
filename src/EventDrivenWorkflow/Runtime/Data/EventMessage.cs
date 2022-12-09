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
    public sealed class EventMessage : WorkflowMessage
    {
        /// <summary>
        /// Gets the message body value.
        /// </summary>
        public EventModel EventModel { get; init; }

        public string WorkflowName { get; init; }

        /// <summary>
        /// Gets the workflow execution context of the message.
        /// </summary>
        public WorkflowExecutionContext WorkflowExecutionContext { get; init; }
    }
}
