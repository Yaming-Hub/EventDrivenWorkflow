// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Diagnostics
{
    using EventDrivenWorkflow.Runtime.Data;

    public interface IWorkflowObserver
    {
        Task WorkflowStarted(WorkflowExecutionContext context);

        Task EventAccepted(QualifiedExecutionContext context, Event @event);

        Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents);

        Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents);

        Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event);

        Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents);

        Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage);

        Task HandleControlMessageFailed(Exception exception, ControlMessage eventMessage);

        Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context);

        Task ActivityExecutionTimeout(QualifiedExecutionContext context);

        Task ControlMessageSent(ControlMessage message);

        Task ControlMessageProcessed(ControlMessage message);

    }
}
