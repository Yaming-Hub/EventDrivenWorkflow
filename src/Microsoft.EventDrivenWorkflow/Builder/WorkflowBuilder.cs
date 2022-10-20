using System;
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityBuilder.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Builder
{
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Utilities;

    /// <summary>
    /// This class help build workflow definition.
    /// </summary>
    public sealed class WorkflowBuilder
    {
        private readonly List<EventBuilder> eventBuilders;
        private readonly List<ActivityBuilder> activityBuilders;
        private readonly List<WorkflowBuilder> childWorkflowBuilders;

        private TimeSpan maxExecuteDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBuilder"/> class.
        /// </summary>
        /// <param name="name">Name of the workflow.</param>
        /// <param name="workflowType">The workflow type.</param>
        public WorkflowBuilder(string name, WorkflowType workflowType = WorkflowType.Static)
        {
            if (StringConstraint.Name.IsValid(name, out string reason))
            {
                throw new ArgumentException($"Workflow name {reason}", paramName: nameof(name));
            }

            this.Name = name;
            this.Type = workflowType;
            this.eventBuilders = new List<EventBuilder>();
            this.activityBuilders = new List<ActivityBuilder>();
            this.childWorkflowBuilders = new List<WorkflowBuilder>();
            this.maxExecuteDuration = TimeSpan.Zero;
        }

        internal string Name { get; }

        internal WorkflowType Type { get; }

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

        public ActivityBuilder AddActivity(string name, bool isAsync = false)
        {
            this.EnsureActivityNameIsUnique(name);

            var activityBuilder = new ActivityBuilder(name, isAsync);
            this.activityBuilders.Add(activityBuilder);
            return activityBuilder;
        }

        public WorkflowBuilder AddWorkflow(string name)
        {
            this.EnsureActivityNameIsUnique(name);

            var childWorkflowBuilder = new WorkflowBuilder(name, this.Type);
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
            var workflowDefinition = this.Build(null, new Dictionary<string, EventDefinition>());

            if (workflowDefinition.ActivityDefinitions.Count == 0)
            {
                throw new InvalidOperationException($"There is no activity defined in workflow {this.Name}.");
            }

            // Check there is no more than one activity subscribe to the same event.
            var eventToSubscribedActivityMap = new Dictionary<string, ActivityDefinition>(workflowDefinition.EventDefinitions.Count);
            foreach (var activityDefinition in workflowDefinition.ActivityDefinitions.Values)
            {
                foreach (var inputEventName in activityDefinition.InputEventDefinitions.Keys)
                {
                    if (eventToSubscribedActivityMap.ContainsKey(inputEventName))
                    {
                        throw new InvalidOperationException(
                            $"The event {inputEventName} is subscribed by both " +
                            $"{eventToSubscribedActivityMap[inputEventName].Name} activity " +
                            $" and {activityDefinition.Name} activity.");
                    }

                    eventToSubscribedActivityMap.Add(inputEventName, activityDefinition);
                }
            }

            // Check every event should be subscribed by one activity.
            if (eventToSubscribedActivityMap.Count < workflowDefinition.EventDefinitions.Count)
            {
                var missingEvents = workflowDefinition.EventDefinitions.Keys.Except(eventToSubscribedActivityMap.Keys).ToList();
                throw new InvalidOperationException($"There are events not be subscribed: {string.Join(",", missingEvents)}");
            }

            // Make sure there is one and only one initializing activity.
            var initializingActivities = workflowDefinition.ActivityDefinitions.Values.Where(a => a.IsInitializing).ToList();
            if (initializingActivities.Count == 0)
            {
                throw new InvalidOperationException($"The workflow {this.Name} has no initializing activity defined.");
            }
            else if (initializingActivities.Count > 1)
            {
                throw new InvalidOperationException(
                    $"The workflow {this.Name} has more than one initializing activity defined: " +
                    $"{string.Join(",", initializingActivities.Select(x => x.Name))}.");
            }

            // Calculate workflow version.
            var signature = workflowDefinition.GetSignature();
            var version = MurmurHash3.HashToString(signature);
            return new WorkflowDefinition
            {
                Name = workflowDefinition.Name,
                Version = version,
                Type = this.Type,
                EventDefinitions = workflowDefinition.EventDefinitions,
                ActivityDefinitions = workflowDefinition.ActivityDefinitions,
                MaxExecuteDuration = this.maxExecuteDuration,
                EventToSubscribedActivityMap = eventToSubscribedActivityMap,
            };
        }

        internal WorkflowDefinition Build(string @namespace, IReadOnlyDictionary<string, EventDefinition> parentEvents)
        {
            // Build events first
            var events = this.eventBuilders.Select(eb => eb.Build()).ToDictionary(e => e.Name, e => e);
            var allEvents = events.Concat(parentEvents).ToDictionary(p => p.Key, p => p.Value);

            var activities = this.activityBuilders.Select(ab => ab.Build(@namespace, allEvents)).ToDictionary(a => a.Name, a => a);

            var childWorkflows = this.childWorkflowBuilders.Select(cwb => cwb.Build(
                @namespace == null ? cwb.Name : $"{@namespace}.{cwb.Name}", allEvents));

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
                MaxExecuteDuration = TimeSpan.Zero
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
