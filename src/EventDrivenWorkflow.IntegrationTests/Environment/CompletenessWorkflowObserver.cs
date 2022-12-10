using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Diagnostics;
using EventDrivenWorkflow.Runtime.Data;
using EventDrivenWorkflow.Utilities;

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    internal sealed class CompletenessWorkflowObserver : IWorkflowObserver
    {
        private TaskCompletionSource TaskCompletionSource;
        private readonly object lockObject;
        private readonly List<string> activeEvents;
        private readonly List<string> activeActivities;

        public CompletenessWorkflowObserver(TaskCompletionSource taskCompletionSource)
        {
            this.TaskCompletionSource = taskCompletionSource;
            this.lockObject = new object();
            this.activeActivities = new List<string>();
            this.activeEvents = new List<string>();
        }

        private static Task Log(string text)
        {
            Trace.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")} {text}");
            return Task.CompletedTask;
        }

        private void CheckComplete(WorkflowExecutionContext workflowExecutionContext)
        {
            Log($"[WorkflowCompletenessTracker] Check complete ActivityCount={this.activeActivities.Count} EventCount={this.activeEvents.Count}.");

            if (this.activeActivities.Count == 0 && this.activeEvents.Count == 0)
            {
                Log($"[WorkflowCompletenessTracker] Workflow execution {workflowExecutionContext.ExecutionId} has completed.");

                this.TaskCompletionSource?.SetResult();
            }
        }

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            lock (this.lockObject)
            {
                this.activeActivities.Remove(context.ActivityExecutionId.GetPath());
                this.CheckComplete(context.WorkflowExecutionContext);
            }

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

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            lock (this.lockObject)
            {
                this.activeActivities.Add(context.ActivityExecutionId.GetPath());
                this.CheckComplete(context.WorkflowExecutionContext);
            }

            return Task.CompletedTask;
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            lock (this.lockObject)
            {
                activeEvents.Remove(@event.GetEventKey(context.WorkflowExecutionContext));
                this.CheckComplete(context.WorkflowExecutionContext);
            }

            return Task.CompletedTask;
        }

        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            lock (this.lockObject)
            {
                activeEvents.Add(@event.GetEventKey(workflowExecutionContext));
                this.CheckComplete(workflowExecutionContext);
            }

            return Task.CompletedTask;
        }

        public Task HandleControlMessageFailed(Exception exception, ControlMessage eventMessage)
        {
            this.TaskCompletionSource?.SetException(exception);

            return Task.CompletedTask;
        }

        public Task HandleEventMessageFailed(Exception exception, EventMessage eventMessage)
        {
            this.TaskCompletionSource?.SetException(exception);

            return Task.CompletedTask;
        }

        public Task WorkflowCompleted(WorkflowExecutionContext context, IEnumerable<Event> outputEvents)
        {
            return Task.CompletedTask;
        }

        public Task WorkflowStarted(WorkflowExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
