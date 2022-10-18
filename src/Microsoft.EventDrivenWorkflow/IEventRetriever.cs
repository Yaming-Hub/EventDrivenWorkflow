// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventRetriever.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a retriever which gets input event payloads for activity.
    /// </summary>
    public interface IEventRetriever
    {
        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <returns>The payload.</returns>
        T GetInputEventPayload<T>(string eventName);
    }
}
