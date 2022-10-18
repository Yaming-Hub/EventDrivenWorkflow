using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    public interface IWorkflowObserver
    {
        void WorkflowStarted(WorkflowExecutionContext context);

        void EventAccepted(WorkflowExecutionContext context, Event @event);

        void ActivityStarted(ActivityExecutionContext context, IEnumerable<Event> inputEvents);

        void ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents);

        void WorkflowCompleted(WorkflowExecutionContext context);
    }
}
