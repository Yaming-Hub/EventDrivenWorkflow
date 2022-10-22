// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    public interface IWorkflowObserver
    {
        Task WorkflowStarted(WorkflowExecutionContext context);

        Task EventAccepted(WorkflowExecutionContext context, Event @event);

        Task ActivityStarting(ExecutionContext context, IEnumerable<Event> inputEvents);

        Task ActivityCompleted(ExecutionContext context, IEnumerable<Event> outputEvents);

        Task EventPublished(ExecutionContext context, Event @event);

        Task WorkflowCompleted(WorkflowExecutionContext context);

        Task HandleEventMessageFailed(Exception exception, Message<EventModel> eventMessage);

        Task HandleControlMessageFailed(Exception exception, Message<ControlModel> eventMessage);

        Task ActivityExecutionFailed(Exception exception, ExecutionContext context);

        Task ActivityExecutionTimeout(ExecutionContext context);
    }
}
