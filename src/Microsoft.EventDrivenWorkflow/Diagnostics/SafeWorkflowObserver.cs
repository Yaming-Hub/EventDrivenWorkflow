// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SafeWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

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

        public Task ActivityCompleted(ExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return IgnoreException(() => observer.ActivityCompleted(context, outputEvents));
        }

        public Task ActivityStarting(ExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return IgnoreException(() => observer.ActivityStarting(context, inputEvents));
        }

        public Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            return IgnoreException(() => observer.EventAccepted(context, @event));
        }

        public Task EventPublished(ExecutionContext context, Event @event)
        {
            return IgnoreException(() => observer.EventPublished(context, @event));
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context)
        {
            return IgnoreException(() => observer.WorkflowCompleted(context));
        }

        public Task HandleEventMessageFailed(Exception context, Message<EventModel> eventMessage)
        {
            return IgnoreException(() => observer.HandleEventMessageFailed(context, eventMessage));
        }

        public Task HandleControlMessageFailed(Exception context, Message<ControlModel> controlMessage)
        {
            return IgnoreException(() => observer.HandleControlMessageFailed(context, controlMessage));
        }

        public Task ActivityExecutionFailed(Exception exception, ExecutionContext context)
        {
            return IgnoreException(() => observer.ActivityExecutionFailed(exception, context));
        }

        public Task ActivityExecutionTimeout(ExecutionContext context)
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
