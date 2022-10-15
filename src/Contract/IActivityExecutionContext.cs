using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;

namespace Microsoft.EventDrivenWorkflow.Contract
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
        /// Gets workflow execution info.
        /// </summary>
        WorkflowExecutionInfo WorkflowExecutionInfo { get; }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <returns>The payload.</returns>
        T GetEventPayload<T>(string eventName);

        /// <summary>
        /// Publish event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        void PublishEvent(string eventName);

        /// <summary>
        /// Publish event after delay.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="delay">How long to delay before the event is published.</param>
        void PublishEvent(string eventName, TimeSpan delay);

        /// <summary>
        /// Publish event.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The payload.</param>
        void PublishEvent<T>(string eventName, T payload);

        /// <summary>
        /// Publish event after delay
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="delay">How long to delay before the event is published.</param>
        void PublishEvent<T>(string eventName, T payload, TimeSpan delay);
    }
}
