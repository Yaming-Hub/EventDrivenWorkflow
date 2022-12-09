// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowOrchestrator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

using EventDrivenWorkflow.Runtime.Data;

namespace EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a publisher which publish output events of activity.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish output event with payload.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        void PublishEvent(string eventName);

        /// <summary>
        /// Publish output event with payload.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="delayDuration">After how long the event will be published.</param>
        void PublishEvent(string eventName, TimeSpan delayDuration);

        /// <summary>
        /// Publish output event with payload.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The event payload.</param>
        void PublishEvent(string eventName, object payload);

        /// <summary>
        /// Publish output event with payload.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The event payload.</param>
        /// <param name="delayDuration">After how long the event will be published.</param>
        void PublishEvent(string eventName, object payload, TimeSpan delayDuration);

    }
}
