using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the callback information. When the parent workflow async activity 
    /// invokes a child workflow and the child workflow completes, it need to notify back to 
    /// the activity to continue. This class contains information for child workflow to notify
    /// back to the parent activity.
    /// </summary>
    public class WorkflowCallbackInfo
    {
        /// <summary>
        /// Gets the execution id of the parent activity to be ended.
        /// </summary>
        public QualifiedActivityExecutionId ActivityExecutionId { get; init; }

        /// <summary>
        /// Gets a dictionary contains the complete event name to parent output event name map.
        /// Note, the event pair both share the same payload type.
        /// </summary>
        public IReadOnlyDictionary<string, string> EventMap { get; init; }
    }
}
