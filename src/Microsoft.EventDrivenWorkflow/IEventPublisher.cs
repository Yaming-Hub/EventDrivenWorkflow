// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowOrchestrator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a publisher which publish output events of activity.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish output event.
        /// </summary>
        /// <param name="events">The outupt events.</param>
        /// <remarks>
        /// This method can only be called from a synchronized activity.
        /// </remarks>
        void PublishEvents(params Event[] events);
    }
}
