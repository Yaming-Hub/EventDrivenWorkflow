// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityReference.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines a reference to an activity.
    /// </summary>
    public sealed class ActivityReference
    {
        /// <summary>
        /// Gets name of the referenced activity.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the execution id.
        /// </summary>
        public Guid ExecutionId { get; init; }
    }
}
