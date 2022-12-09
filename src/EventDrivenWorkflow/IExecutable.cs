// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExecutable.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow
{
    using EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This interface defines a workflow activity execution logic which is the atom operation in a workflow.
    /// </summary>
    public interface IExecutable
    {
        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="eventRetriever">The event retriever used to get input event payloads.</param>
        /// <param name="eventPublisher">The event publisher used to publish output events.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task represents the async operation.</returns>
        Task Execute(
            QualifiedExecutionContext context,
            IEventRetriever eventRetriever,
            IEventPublisher eventPublisher,
            CancellationToken cancellationToken);
    }
}
