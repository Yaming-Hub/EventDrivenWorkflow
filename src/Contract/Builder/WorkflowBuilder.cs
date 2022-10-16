using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;

namespace Microsoft.EventDrivenWorkflow.Contract.Builder
{
    public class WorkflowBuilder
    {
        private readonly List<EventBuilder> eventBuilders;
        private readonly List<ActivityBuilder> activityBuilders;
        private readonly List<ChildWorkflowBuilder> childWorkflowBuilders;

        private TimeSpan maxExecuteDuration;

        internal WorkflowBuilder(string name)
        {
            if (StringConstraint.Name.IsValid(name, out string reason))
            {
                throw new ArgumentException($"Workflow name {reason}", paramName: nameof(name));
            }

            this.Name = name;
            this.eventBuilders = new List<EventBuilder>();
            this.activityBuilders = new List<ActivityBuilder>();
            this.maxExecuteDuration = TimeSpan.Zero;
        }

        public string Name { get; }

        internal IReadOnlyList<EventBuilder> EventBuilders => this.eventBuilders;

        public EventBuilder RegisterEvent(string name)
        {
            if (this.eventBuilders.Any(eb => eb.Name == name))
            {
                throw new InvalidOperationException($"Event {name} is already registered.");
            }

            var eventBuilder = new EventBuilder(name, payloadType: null);
            this.eventBuilders.Add(eventBuilder);
            return eventBuilder;
        }

        public EventBuilder RegisterEvent<T>(string name)
        {
            if (this.eventBuilders.Any(eb => eb.Name == name))
            {
                throw new InvalidOperationException($"Event {name} is already registered.");
            }

            var eventBuilder = new EventBuilder(name, payloadType: typeof(T));
            this.eventBuilders.Add(eventBuilder);
            return eventBuilder;
        }

        public ActivityBuilder AddActivity(string name)
        {
            this.EnsureActivityNameIsUnique(name);

            var activityBuilder = new ActivityBuilder(name);
            this.activityBuilders.Add(activityBuilder);
            return activityBuilder;
        }

        public ChildWorkflowBuilder AddWorkflow(string name)
        {
            this.EnsureActivityNameIsUnique(name);

            var childWorkflowBuilder = new ChildWorkflowBuilder(name);
            this.childWorkflowBuilders.Add(childWorkflowBuilder);
            return childWorkflowBuilder;
        }

        public WorkflowBuilder SetMaxExecuteDuration(TimeSpan duration)
        {
            this.maxExecuteDuration = duration;
            return this;
        }

        public WorkflowDefinition Build()
        {
            return this.Build(this.Name, new Dictionary<string, EventDefinition>());
        }

        internal WorkflowDefinition Build(string parentFullName, IReadOnlyDictionary<string, EventDefinition> parentEvents)
        {
            // Build events first
            var events = this.eventBuilders.Select(eb => eb.Build()).ToDictionary(e => e.Name, e => e);
            var allEvents = events.Concat(parentEvents).ToDictionary(p => p.Key, p => p.Value);

            var activities = this.activityBuilders.Select(ab => ab.Build(parentFullName, allEvents)).ToDictionary(a => a.Name, a => a);

            var childWorkflows = this.childWorkflowBuilders.Select(cwb => cwb.Build(parentFullName, allEvents));

            foreach (var childWorkflow in childWorkflows)
            {
                foreach (var childEvent in childWorkflow.EventDefinitions.Values)
                {
                    // Event name should be unique within the scope of the root workflow.
                    events.Add(childEvent.Name, childEvent);
                }

                foreach (var childActivity in childWorkflow.ActivityDefinitions.Values)
                {
                    // Child activity name will be in {parent}.{name} format, so it should also unique within the root workflow.
                    activities.Add(childActivity.Name, childActivity);
                }
            }

            return new WorkflowDefinition
            {
                Name = this.Name,
                Version = String.Empty,
                EventDefinitions = events,
                ActivityDefinitions = activities,
                MaxExecuteDuration = this.maxExecuteDuration
            };
        }

        private void EnsureActivityNameIsUnique(string name)
        {
            if (this.activityBuilders.Any(ab => ab.Name == name))
            {
                throw new InvalidOperationException($"Activity {name} is already registered.");
            }

            if (this.childWorkflowBuilders.Any(wb => wb.Name == name))
            {
                throw new InvalidOperationException($"Activity {name} is already registered.");
            }
        }
    }
}
