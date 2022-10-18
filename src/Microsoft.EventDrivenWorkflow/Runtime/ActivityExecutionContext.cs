// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityExecutionContext.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This class defines the context information of the executing activity.
    /// </summary>
    public sealed class ActivityExecutionContext : WorkflowExecutionContext
    {
        /// <summary>
        /// Gets the activity name.
        /// </summary>
        public string ActivityName { get; init; }

        /// <summary>
        /// Gets the execution id of the activity.
        /// </summary>
        public Guid ActivityExecutionId { get; init; }

        /// <summary>
        /// Gets the start time of the execution.
        /// </summary>
        public DateTime ActivityExecutionStartDateTime { get; init; }
    }
}
