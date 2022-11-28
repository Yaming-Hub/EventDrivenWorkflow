// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidWorkflowException.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This class defines an exception which represents an invalid workflow definition.
    /// </summary>
    public sealed class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
