using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core
{
    public sealed class WorkflowOptions
    {
        /// <summary>
        /// Gets a value indicates whether to ignore the event if it's published by previous version 
        /// of the workflow activity. Set this value to true if there is a breaking change in the workflow
        /// definition to avoid unexpected behavior. The previous workflow may timeout as events are
        /// ignored.
        /// </summary>
        public bool IgnoreEventIfVersionMismatches { get; init; } = false;

        /// <summary>
        /// Gets a value indicates whether to track finish and timeout for the workflow.
        /// </summary>
        public bool TrackWorkflow { get; init; } = false;
    }
}
