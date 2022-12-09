﻿// --------------------------------------------------------------------------------------------------------------------
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

        Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents);

        Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents);

        Task EventPublished(QualifiedExecutionContext context, Event @event);

        Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents);

        Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage);

        Task HandleControlMessageFailed(Exception exception, ControlMessage eventMessage);

        Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context);

        Task ActivityExecutionTimeout(QualifiedExecutionContext context);
    }
}
