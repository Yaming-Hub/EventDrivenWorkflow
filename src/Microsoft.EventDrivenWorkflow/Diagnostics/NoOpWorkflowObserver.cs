using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime.Data;

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

        public Task HandleEventMessageFailed(Exception context, Message<EventModel> eventMessage)
        {
            return Task.CompletedTask;
        }

        public Task HandleControlMessageFailed(Exception context, Message<ControlModel> controlMessage)
        {
            return Task.CompletedTask;
        }

        public Task ActivityExecutionFailed(Exception exception, ActivityExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ActivityExecutionTimeout(ActivityExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
