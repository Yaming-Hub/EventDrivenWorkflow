// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventModel.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the data model represents a persistable event.
    /// </summary>
    public sealed class EventModel
    {
        /// <summary>
        /// Gets id of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets name of the event.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the event payload.
        /// </summary>
        public Payload Payload { get; init; }

        /// <summary>
        /// Gets the delay duration of the event.
        /// </summary>
        public TimeSpan DelayDuration { get; init; }

        /// <summary>
        /// Gets id of the engine where this event is created.
        /// </summary>
        public string SourceEngineId { get; set; }

        /// <summary>
        /// Gets the source activity from where the message is sent.
        /// </summary>
        public ActivityReference SourceActivity { get; init; }
    }
}
