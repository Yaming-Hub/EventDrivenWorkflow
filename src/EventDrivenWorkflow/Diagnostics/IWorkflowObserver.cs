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

        Task EventAccepted(WorkflowExecutionContext context, Event @event);

        Task ActivityStarting(ExecutionContext context, IEnumerable<Event> inputEvents);

        Task ActivityCompleted(ExecutionContext context, IEnumerable<Event> outputEvents);

        Task EventPublished(ExecutionContext context, Event @event);

        Task WorkflowCompleted(WorkflowExecutionContext context);

        Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage);

        Task HandleControlMessageFailed(Exception exception, ControlMessage eventMessage);

        Task ActivityExecutionFailed(Exception exception, ExecutionContext context);

        Task ActivityExecutionTimeout(ExecutionContext context);
    }
}
