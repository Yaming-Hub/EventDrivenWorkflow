﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Diagnostics;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
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

        public Task ActivityStarting(ActivityExecutionContext context, IEnumerable<Event> inputEvents)
        {
            return Log($"ActivityStarting   Activity={context.GetPath()} Events={string.Join(",", inputEvents.Select(x => x.Name))}");
        }

        public Task ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Log($"ActivityCompleted  Activity={context.GetPath()} Events={string.Join(",", outputEvents.Select(x => x.Name))}");
        }

        public Task EventPublished(WorkflowExecutionContext context, Event @event)
        {
            return Log($"EventAccepted      Activity={context.GetPath()} Event={@event.Name}");
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context)
        {
            return Log($"WorkflowCompleted  Workflow={context.GetPath()}");
        }

        private static Task Log(string text)
        {
            Trace.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")} {text}");
            return Task.CompletedTask;
        }
    }
}