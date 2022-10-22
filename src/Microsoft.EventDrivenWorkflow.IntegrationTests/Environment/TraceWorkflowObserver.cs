// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceWorkflowObserver.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    using System.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    public class TraceWorkflowObserver : IWorkflowObserver
    {
        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return Log($"WorkflowStarted    Workflow={context.GetPath()}");
        }

        public Task EventAccepted(WorkflowExecutionContext context, Event @event)
        {
            return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
        }

        public Task ActivityStarting(ExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Log($"ActivityStarting   Activity={context.GetPath()} Events={string.Join(",", inputEvents.Select(x => x.Name))}");
        }

        public Task ActivityCompleted(ExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Log($"ActivityCompleted  Activity={context.GetPath()} Events={string.Join(",", outputEvents.Select(x => x.Name))}");
        }

        public Task EventPublished(ExecutionContext context, Event @event)
        {
            return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context)
        {
            return Log($"WorkflowCompleted  Workflow={context.GetPath()}");
        }

        public Task HandleEventMessageFailed(Exception exception, Message<EventModel> eventMessage)
        {
            return Log($"HandleEventMessageFailed {exception}");
        }

        public Task HandleControlMessageFailed(Exception exception, Message<ControlModel> eventMessage)
        {
            return Log($"HandleControlMessageFailed {exception}");
        }

        public Task ActivityExecutionFailed(Exception exception, ExecutionContext context)
        {
            return Log($"ActivityExecutionFailed {exception} Activity={context.GetPath()}");
        }

        public Task ActivityExecutionTimeout(ExecutionContext context)
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
