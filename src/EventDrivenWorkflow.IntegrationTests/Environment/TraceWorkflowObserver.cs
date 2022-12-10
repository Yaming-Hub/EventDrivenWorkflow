// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    using EventDrivenWorkflow.Diagnostics;
    using EventDrivenWorkflow.Runtime.Data;
    using static EventDrivenWorkflow.IntegrationTests.Environment.TestLogger;

    public class TraceWorkflowObserver : IWorkflowObserver
    {
        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            Log(nameof(TraceWorkflowObserver), "WorkflowStarted", $"Workflow={context.GetPath()}");
            return Task.CompletedTask;
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            Log(nameof(TraceWorkflowObserver), "EventAccepted", $"Activity={context.GetPath()} Event={@event.Name}");
            return Task.CompletedTask;
        }

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            Log(nameof(TraceWorkflowObserver), "ActivityStarting", $"Activity={context.GetPath()} Events={string.Join(",", inputEvents.Select(x => x.Name))}");
            return Task.CompletedTask;
        }

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            Log(nameof(TraceWorkflowObserver), "ActivityCompleted", $"Activity={context.GetPath()} Events={string.Join(",", outputEvents.Select(x => x.Name))}");
            return Task.CompletedTask;
        }

        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            string activityInfo = $"{activityExecutionContext?.ActivityName}/{activityExecutionContext?.ActivityId}";
            Log(nameof(TraceWorkflowObserver), "EventPublished", $"Workflow={workflowExecutionContext.GetPath()} Activity={activityInfo} Event={@event.Name}");
            return Task.CompletedTask;
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            Log(nameof(TraceWorkflowObserver), "WorkflowCompleted", $"Workflow={context.GetPath()}");
            return Task.CompletedTask;
        }

        public Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage)
        {
            Log(nameof(TraceWorkflowObserver), "HandleEventMessageFailed", $"exception={exception}");
            return Task.CompletedTask;
        }

        public Task HandleControlMessageFailed(Exception exception, ControlMessage controlMessage)
        {
            Log(nameof(TraceWorkflowObserver), "HandleControlMessageFailed", $"exception={exception}");
            return Task.CompletedTask;
        }

        public Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            Log(nameof(TraceWorkflowObserver), "ActivityExecutionFailed", $"exception={exception} Activity={context.GetPath()}");
            return Task.CompletedTask;
        }

        public Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            Log(nameof(TraceWorkflowObserver), "ActivityExecutionTimeout", $"Activity={context.GetPath()}");
            return Task.CompletedTask;
        }

        public Task ControlMessageSent(ControlMessage message)
        {
            Log(nameof(TraceWorkflowObserver), "ControlMessageSent", $"Operation={message.Operation}");
            return Task.CompletedTask;
        }

        public Task ControlMessageProcessed(ControlMessage message)
        {
            Log(nameof(TraceWorkflowObserver), "ControlMessageProcessed", $"Operation={message.Operation}");
            return Task.CompletedTask;
        }
    }
}
