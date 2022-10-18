using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines the execution context information.
    /// </summary>
    public interface IActivityExecutionContext
    {
        /// <summary>
        /// Gets the workflow definition.
        /// </summary>
        WorkflowDefinition WorkflowDefinition { get; }

        /// <summary>
        /// Gets the activity definition.
        /// </summary>
        ActivityDefinition ActivityDefinition { get; }

        /// <summary>
        /// Gets activity execution info.
        /// </summary>
        ActivityExecutionInfo ActivityExecutionInfo { get; }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <returns>The payload.</returns>
        T GetInputEventPayload<T>(string eventName);

        /// <summary>
        /// Publish output event.
        /// </summary>
        /// <param name="events">The outupt events.</param>
        /// <remarks>
        /// This method can only be called from sync activity.
        /// </remarks>
        void PublishEvent(params Event[] events);
    }
}
