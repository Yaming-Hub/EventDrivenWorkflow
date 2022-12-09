// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoOpWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------

namespace EventDrivenWorkflow.Diagnostics
{
    using EventDrivenWorkflow.Runtime.Data;

    internal sealed class NoOpWorkflowObserver : IWorkflowObserver
    {
        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ActivityCompleted(ExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Task.CompletedTask;
        }

        public Task ActivityStarting(ExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Task.CompletedTask;
        }

        public Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            return Task.CompletedTask;
        }
        public Task EventPublished(ExecutionContext context, Event @event)
        {
            return Task.CompletedTask;
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
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

        public Task ActivityExecutionFailed(Exception exception, ExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ActivityExecutionTimeout(ExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
