// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActivityFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a factory which creates activities of a workflow.
    /// </summary>
    public interface IActivityFactory
    {
        /// <summary>
        /// Creates a synchronous activity.
        /// </summary>
        /// <param name="name">The activity name.</param>
        /// <returns>The synchronous activity instance.</returns>
        IActivity CreateActivity(string name);

        /// <summary>
        /// Creates an asynchronous activity.
        /// </summary>
        /// <param name="name">The activity name.</param>
        /// <returns>The asynchronous activity instance.</returns>
        IAsyncActivity CreateAsyncActivity(string name);
    }
}
