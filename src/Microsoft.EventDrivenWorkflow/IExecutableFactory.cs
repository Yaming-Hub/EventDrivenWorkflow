// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExecutableFactory.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a factory which creates executables of a workflow.
    /// </summary>
    public interface IExecutableFactory
    {
        /// <summary>
        /// Creates a synchronous executable for the activity.
        /// </summary>
        /// <param name="name">The activity name.</param>
        /// <returns>The synchronous executable instance.</returns>
        IExecutable CreateExecutable(string name);

        /// <summary>
        /// Creates an asynchronous executable for the activity.
        /// </summary>
        /// <param name="name">The activity name.</param>
        /// <returns>The asynchronous executable instance.</returns>
        IAsyncExecutable CreateAsyncExecutable(string name);
    }
}
