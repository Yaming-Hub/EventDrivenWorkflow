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

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Task.CompletedTask;
        }

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Task.CompletedTask;
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            return Task.CompletedTask;
        }
        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            return Task.CompletedTask;
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Task.CompletedTask;
        }

        public Task HandleEventMessageFailed(Exception context, EventMessage eventMessage)
        {
            return Task.CompletedTask;
        }

        public Task HandleControlMessageFailed(Exception context, ControlMessage controlMessage)
        {
            return Task.CompletedTask;
        }

        public Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
