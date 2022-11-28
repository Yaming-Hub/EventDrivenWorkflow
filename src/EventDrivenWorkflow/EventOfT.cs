// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Event.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow
{
    /// <summary>
    /// This class defines an event with payload the activity can publish.
    /// </summary>
    /// <typeparam name="T">Type of the payload.</typeparam>
    public sealed class Event<T> : Event
    {
        /// <summary>
        /// Gets the payload.
        /// </summary>
        public T Payload { get; init; }
    }
}
