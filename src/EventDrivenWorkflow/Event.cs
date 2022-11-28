// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Event.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow
{
    /// <summary>
    /// This class defines an event the activity can publish.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets event id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets name of the event.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets after how long the event should be published.
        /// </summary>
        public TimeSpan DelayDuration { get; init; }

        /// <summary>
        /// Gets the source engine id.
        /// </summary>
        public string SourceEngineId { get; init; }
    }
}
