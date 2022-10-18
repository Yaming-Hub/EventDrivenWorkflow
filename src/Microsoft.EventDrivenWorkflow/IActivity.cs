// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActivity.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    using Microsoft.EventDrivenWorkflow.Runtime;

    /// <summary>
    /// This interface defines a workflow activity. The activity is the atom operation in a workflow.
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task represents the async operation.</returns>
        Task Execute(ActivityExecutionContext context, CancellationToken cancellationToken);
    }
}
