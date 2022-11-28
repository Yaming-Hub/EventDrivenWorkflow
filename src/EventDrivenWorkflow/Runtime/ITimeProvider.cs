// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITimeProvider.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This interface defines a time provider.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets current UTC time.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
