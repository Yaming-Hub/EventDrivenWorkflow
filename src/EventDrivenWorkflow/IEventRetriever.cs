// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventRetriever.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a retriever which gets input event payloads for activity.
    /// </summary>
    public interface IEventRetriever
    {
        /// <summary>
        /// Gets event by name.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <returns>The event.</returns>
        Event GetEvent(string eventName);

        T GetEventValue<T>(string eventName);
    }
}
