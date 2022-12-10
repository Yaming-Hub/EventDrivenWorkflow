using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Diagnostics;
using EventDrivenWorkflow.Runtime.Data;
using EventDrivenWorkflow.Utilities;
using static EventDrivenWorkflow.IntegrationTests.Environment.TestLogger;

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    internal sealed class CompletenessWorkflowObserver : IWorkflowObserver
    {
        private TaskCompletionSource TaskCompletionSource;
        private readonly object lockObject;
        private readonly List<string> events;
        private readonly List<string> activities;
        private int controlMessageCount;

        public CompletenessWorkflowObserver(TaskCompletionSource taskCompletionSource)
        {
            this.TaskCompletionSource = taskCompletionSource;
            this.lockObject = new object();
            this.activities = new List<string>();
            this.events = new List<string>();
            this.controlMessageCount = 0;
        }

        private void CheckComplete()
        {
            Log(nameof(CompletenessWorkflowObserver), "CheckComplete", $"ActivityCount={this.activities.Count} EventCount={this.events.Count} ControlCount={this.controlMessageCount}.");

            if (this.activities.Count == 0 && this.events.Count == 0 && this.controlMessageCount == 0)
            {
                Log(nameof(CompletenessWorkflowObserver), "Completed", $"There is no pending or active execution.");

                this.TaskCompletionSource?.SetResult();
            }
        }

        public Task ActivityCompleted(QualifiedExecutionContext context, IEnumerable<Event> outputEvents)
        {
            lock (this.lockObject)
            {
                this.activities.Remove(context.ActivityExecutionId.GetPath());
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task ActivityExecutionFailed(Exception exception, QualifiedExecutionContext context)
        {
            lock (this.lockObject)
            {
                this.activities.Remove(context.ActivityExecutionId.GetPath());
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task ActivityExecutionTimeout(QualifiedExecutionContext context)
        {
            lock (this.lockObject)
            {
                this.activities.Remove(context.ActivityExecutionId.GetPath());
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task ActivityStarting(QualifiedExecutionContext context, IEnumerable<Event> inputEvents)
        {
            lock (this.lockObject)
            {
                this.activities.Add(context.ActivityExecutionId.GetPath());
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task EventAccepted(QualifiedExecutionContext context, Event @event)
        {
            lock (this.lockObject)
            {
                events.Remove(@event.GetEventKey(context.WorkflowExecutionContext));
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task EventPublished(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext activityExecutionContext, Event @event)
        {
            lock (this.lockObject)
            {
                events.Add(@event.GetEventKey(workflowExecutionContext));
                this.CheckComplete();
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

        public Task ControlMessageSent(ControlMessage message)
        {
            lock (this.lockObject)
            {
                this.controlMessageCount++;
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }

        public Task ControlMessageProcessed(ControlMessage message)
        {
            lock (this.lockObject)
            {
                this.controlMessageCount--;
                this.CheckComplete();
            }

            return Task.CompletedTask;
        }
    }
}
