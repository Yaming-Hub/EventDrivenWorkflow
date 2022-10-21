// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityReference.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the readiness state of an activity.
    /// </summary>
    public sealed class ActivityState
    {
        /// <summary>
        /// Gets the workflow name.
        /// </summary>
        public string WorkflowName { get; init; }

        /// <summary>
        /// Gets the workflow version.
        /// </summary>
        public string WorkflowVersion { get; init; }

        /// <summary>
        /// Gets the workflow id.
        /// </summary>
        public Guid WorkflowId { get; init; }

        /// <summary>
        /// Gets the activity name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets or sets the list of inputs that is available.
        /// </summary>
        public List<string> AvailableInputEvents { get; init; }
    }
}
