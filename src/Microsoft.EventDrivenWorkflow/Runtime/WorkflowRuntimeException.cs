// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowException.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This class defines a workflow runtime exception.
    /// </summary>
    internal sealed class WorkflowRuntimeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowRuntimeException"/> instance.
        /// </summary>
        /// <param name="isTransient">Whether the failure is transient or not.</param>
        /// <param name="message">The error message.</param>
        public WorkflowRuntimeException(bool isTransient, string message)
            : this(isTransient, message, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowRuntimeException"/> instance.
        /// </summary>
        /// <param name="isTransient">Whether the failure is transient or not.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public WorkflowRuntimeException(bool isTransient, string message, Exception innerException)
            : base(message, innerException)
        {
            this.IsTransient = isTransient;
        }

        /// <summary>
        /// Gets a value indicates whether the failure is transient or not.
        /// </summary>
        public bool IsTransient { get; }
    }
}
