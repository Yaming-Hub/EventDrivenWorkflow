// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    using System.Diagnostics;
    using EventDrivenWorkflow.Diagnostics;
    using EventDrivenWorkflow.Runtime.Data;

    public class TraceWorkflowObserver : IWorkflowObserver
    {
        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return Log($"WorkflowStarted    Workflow={context.GetPath()}");
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
        }

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Log($"ActivityStarting   Activity={context.GetPath()} Events={string.Join(",", inputEvents.Select(x => x.Name))}");
        }

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Log($"ActivityCompleted  Activity={context.GetPath()} Events={string.Join(",", outputEvents.Select(x => x.Name))}");
        }

        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            string activityInfo = $"{activityExecutionContext?.ActivityName}/{activityExecutionContext?.ActivityId}";
            return Log($"EventPublished      Workflow={workflowExecutionContext.GetPath()} Activity={activityInfo} Event={@event.Name}");
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Log($"WorkflowCompleted  Workflow={context.GetPath()}");
        }

        public Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage)
        {
            return Log($"HandleEventMessageFailed {exception}");
        }

        public Task HandleControlMessageFailed(Exception exception, ControlMessage controlMessage)
        {
            return Log($"HandleControlMessageFailed {exception}");
        }

        public Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            return Log($"ActivityExecutionFailed {exception} Activity={context.GetPath()}");
        }

        public Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            return Log($"ActivityExecutionTimeout Activity={context.GetPath()}");
        }

        private static Task Log(string text)
        {
            Trace.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")} {text}");
            return Task.CompletedTask;
        }
    }
}
