using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    public class ActivityTraceEvent
    {
        public WorkflowDefinition WorkflowDefinition { get; init; }

        public ActivityDefinition ActivityDefinition { get; init; }

        public ActivityExecutionContext ActivityExecutionContext { get; init; }
    }
}
