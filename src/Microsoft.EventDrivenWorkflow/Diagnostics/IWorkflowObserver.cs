﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;
using Microsoft.EventDrivenWorkflow.Runtime.Data;

namespace Microsoft.EventDrivenWorkflow.Diagnostics
{
    public interface IWorkflowObserver
    {
        Task WorkflowStarted(WorkflowExecutionContext context);

        Task EventAccepted(WorkflowExecutionContext context, Event @event);

        Task ActivityStarting(ActivityExecutionContext context, IEnumerable<Event> inputEvents);

        Task ActivityCompleted(ActivityExecutionContext context, IEnumerable<Event> outputEvents);

        Task EventPublished(WorkflowExecutionContext context, Event @event);

        Task WorkflowCompleted(WorkflowExecutionContext context);

        Task HandleEventMessageFailed(Exception exception, Message<EventModel> eventMessage);

        Task HandleControlMessageFailed(Exception exception, Message<ControlModel> eventMessage);

        Task ActivityExecutionFailed(Exception exception, ActivityExecutionContext context);

        Task ActivityExecutionTimeout(ActivityExecutionContext context);
    }
}
