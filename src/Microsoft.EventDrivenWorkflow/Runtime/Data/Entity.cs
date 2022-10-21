// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Entity.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
{
    using Microsoft.EventDrivenWorkflow.Persistence;

    /// <summary>
    /// This class defines an generic entity type.
    /// </summary>
    /// <typeparam name="T">Type of the entity value.</typeparam>
    public sealed class Entity<T> : IEntity
    {
        /// <summary>
        /// Gets the value of the entity.
        /// </summary>
        public T Value { get; init; }

        /// <inheritdoc/>
        public string ETag { get; set; }

        /// <inheritdoc/>
        public DateTime ExpireDateTime { get; init; }
    }
}
