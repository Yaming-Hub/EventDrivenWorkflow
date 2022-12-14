// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SafeWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Diagnostics
{
    using System.Diagnostics;
    using EventDrivenWorkflow.Runtime.Data;

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

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return IgnoreException(() => observer.ActivityCompleted(context, outputEvents));
        }

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return IgnoreException(() => observer.ActivityStarting(context, inputEvents));
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            return IgnoreException(() => observer.EventAccepted(context, @event));
        }

        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            return IgnoreException(() => observer.EventPublished(workflowExecutionContext, activityExecutionContext, @event));
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return IgnoreException(() => observer.WorkflowCompleted(context, outputEvents));
        }

        public Task HandleEventMessageFailed(Exception context, EventMessage eventMessage)
        {
            return IgnoreException(() => observer.HandleEventMessageFailed(context, eventMessage));
        }

        public Task HandleControlMessageFailed(Exception context, ControlMessage controlMessage)
        {
            return IgnoreException(() => observer.HandleControlMessageFailed(context, controlMessage));
        }

        public Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            return IgnoreException(() => observer.ActivityExecutionFailed(exception, context));
        }

        public Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            return IgnoreException(() => observer.ActivityExecutionTimeout(context));
        }

        public Task ControlMessageSent(ControlMessage message)
        {
            return IgnoreException(() => observer.ControlMessageSent(message));
        }

        public Task ControlMessageProcessed(ControlMessage message)
        {
            return IgnoreException(() => observer.ControlMessageProcessed(message));
        }

        private async Task IgnoreException(Func<Task> observerAction)
        {
            try
            {
                await observerAction();
            }
            catch (Exception e)
            {
                // The workflow orchestration should not be impacted by observer behaviors
                // so here exception thrown from observer action will be ignored.
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
