using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime;
using Microsoft.EventDrivenWorkflow.Runtime.Model;

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    internal sealed class SafeWorkflowObserver : IWorkflowObserver
    {
        private readonly IWorkflowObserver observer;

        public SafeWorkflowObserver(IWorkflowObserver observer)
        {
            this.observer = observer ?? new NoOpWorkflowObserver();
        }

        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return IgnoreException(() => observer.WorkflowStarted(context));
        }

        public Task ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return IgnoreException(() => observer.ActivityCompleted(context, outputEvents));
        }

        public Task ActivityStarting(ActivityExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return IgnoreException(() => observer.ActivityStarting(context, inputEvents));
        }

        public Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            return IgnoreException(() => observer.EventAccepted(context, @event));
        }

        public Task EventPublished(WorkflowExecutionContext context, Event @event)
        {
            return IgnoreException(() => observer.EventPublished(context, @event));
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context)
        {
            return IgnoreException(() => observer.WorkflowCompleted(context));
        }

        public Task HandleEventMessageFailed(Exception context, EventMessage eventMessage)
        {
            return IgnoreException(() => observer.HandleEventMessageFailed(context, eventMessage));
        }

        public Task HandleControlMessageFailed(Exception context, ControlMessage controlMessage)
        {
            return IgnoreException(() => observer.HandleControlMessageFailed(context, controlMessage));
        }

        public Task ActivityExecutionFailed(Exception exception, ActivityExecutionContext context)
        {
            return IgnoreException(() => observer.ActivityExecutionFailed(exception, context));
        }

        public Task ActivityExecutionTimeout(ActivityExecutionContext context)
        {
            return IgnoreException(() => observer.ActivityExecutionTimeout(context));
        }

        private async Task IgnoreException(Func<Task> observerAction)
        {
            try
            {
                await observerAction();
            }
            catch
            {
                // The workflow orchestration should not be impacted by observer behaviors
                // so here exception thrown from observer action will be ignored.
            }
        }
    }
}
