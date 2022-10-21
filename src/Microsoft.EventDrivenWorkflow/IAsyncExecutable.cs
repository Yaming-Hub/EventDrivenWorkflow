// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAsyncExecutable.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    using Microsoft.EventDrivenWorkflow.Runtime;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    /// <summary>
    /// This interface defines an asynchronous executable. The asynchronous activity is useful when the
    /// activity triggers an external operation and rely on callback of the external system to complete.
    /// For example, if the activity startds an Azure Data Factory pipeline, upon completion the ADF
    /// pipeline notifies the application. In this case, the notification handler will call workflow
    /// orchestrator to complete the activity and move on.
    /// </summary>
    public interface IAsyncExecutable
    {
        /// <summary>
        /// Begin execute the activity, the activity will remain executing after this event completes.
        /// Use <see cref="WorkflowOrchestrator.EndExecute(ActivityExecutionContext, Event[])"/> method to 
        /// complete the event execution.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="eventRetriever">The event retriever used to get input event payloads.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task represents the async operation.</returns>
        Task BeginExecute(
            ActivityExecutionContext context,
            IEventRetriever eventRetriever);
    }
}
