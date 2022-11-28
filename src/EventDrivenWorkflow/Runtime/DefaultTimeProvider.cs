// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultTimeProvider.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This class defines a default time provider.
    /// </summary>
    internal sealed class DefaultTimeProvider : ITimeProvider
    {
        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
