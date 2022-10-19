using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    internal sealed class NoOpWorkflowObserver : IWorkflowObserver
    {
        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Task.CompletedTask;
        }

        public Task ActivityStarting(ActivityExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Task.CompletedTask;
        }

        public Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            return Task.CompletedTask;
        }
        public Task EventPublished(WorkflowExecutionContext context, Event @event)
        {
            return Task.CompletedTask;
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
