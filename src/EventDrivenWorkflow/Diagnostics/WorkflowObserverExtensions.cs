using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Runtime.Data;

namespace EventDrivenWorkflow.Diagnostics
{
    public static class WorkflowObserverExtensions
    {
        public static Task EventPublished(this IWorkflowObserver observer, QualifiedExecutionContext context, Event @event)
        {
            return observer.EventPublished(context.WorkflowExecutionContext, context.ActivityExecutionContext, @event);
        }
    }
}
