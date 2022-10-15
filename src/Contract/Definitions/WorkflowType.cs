using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Definitions
{
    public enum WorkflowType
    {
        /// <summary>
        /// A open workflow doesn't have initializing or terminating activities. The workflow works
        /// as an activity which has it's own input events and output events. The open workflow is
        /// typically used as sub workflow to construct complex workflow.
        /// </summary>
        Open,

        /// <summary>
        /// A close workflow has single initializing activity, one or more terminating activities.
        /// Each event must be subscribed and published activities of the workflow.
        /// </summary>
        Close
    }
}
