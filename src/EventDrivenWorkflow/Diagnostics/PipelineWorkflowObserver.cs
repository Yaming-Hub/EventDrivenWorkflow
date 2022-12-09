using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Runtime.Data;

namespace EventDrivenWorkflow.Diagnostics
{
    public sealed class PipelineWorkflowObserver : IWorkflowObserver
    {
        private readonly IReadOnlyList<IWorkflowObserver> observers;
        public PipelineWorkflowObserver(params IWorkflowObserver[] observers)
            : this((IReadOnlyList<IWorkflowObserver>)observers)
        {
        }

        public PipelineWorkflowObserver(IReadOnlyList<IWorkflowObserver> observers)
        {
            this.observers = observers;
        }

        public async Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            foreach(var observer in observers) 
            {
                await observer.ActivityCompleted(context, outputEvents);
            }
        }

        public async Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            foreach (var observer in observers)
            {
                await observer.ActivityExecutionFailed(exception, context);
            }
        }

        public async Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            foreach (var observer in observers)
            {
                await observer.ActivityExecutionTimeout(context);
            }
        }

        public async Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            foreach (var observer in observers)
            {
                await observer.ActivityStarting(context, inputEvents);
            }
        }

        public async Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            foreach (var observer in observers)
            {
                await observer.EventAccepted(context, @event);
            }
        }

        public async Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            foreach (var observer in observers)
            {
                await observer.EventPublished(workflowExecutionContext, activityExecutionContext, @event);
            }
        }

        public async Task HandleControlMessageFailed(Exception exception, ControlMessage eventMessage)
        {
            foreach (var observer in observers)
            {
                await observer.HandleControlMessageFailed(exception, eventMessage);
            }
        }

        public async Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage)
        {
            foreach (var observer in observers)
            {
                await observer.HandleEventMessageFailed(exception, eventMessage);
            }
        }

        public async Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            foreach (var observer in observers)
            {
                await observer.WorkflowCompleted(context, outputEvents);
            }
        }

        public async Task WorkflowStarted(WorkflowExecutionContext context)
        {
            foreach (var observer in observers)
            {
                await observer.WorkflowStarted(context);
            }
        }
    }
}
