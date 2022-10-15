using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public interface IAsyncActivityExecutionContext
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
    }
}
