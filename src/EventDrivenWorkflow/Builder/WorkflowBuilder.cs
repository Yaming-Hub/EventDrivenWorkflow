// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowBuilder.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Builder
{
    using System;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Utilities;

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
        public WorkflowBuilder(string name, WorkflowType workflowType)
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
            this.maxExecuteDuration = TimeSpan.FromDays(1);
        }

        internal string Name { get; }

        internal WorkflowType Type { get; }

        internal IReadOnlyList<EventBuilder> EventBuilders => this.eventBuilders;

        public EventBuilder RegisterEvent(string name)
        {
            if (this.eventBuilders.Any(eb => eb.Name == name))
            {
                throw new InvalidWorkflowException($"Event {name} is already registered.");
            }

            var eventBuilder = new EventBuilder(name, payloadType: null);
            this.eventBuilders.Add(eventBuilder);
            return eventBuilder;
        }

        public EventBuilder RegisterEvent<T>(string name)
        {
            if (this.eventBuilders.Any(eb => eb.Name == name))
            {
                throw new InvalidWorkflowException($"Event {name} is already registered.");
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
                throw new InvalidWorkflowException($"There is no activity defined in workflow {this.Name}.");
            }

            // Check there is no more than one activity subscribe to the same event.
            var eventToSubscribedActivityMap = new Dictionary<string, ActivityDefinition>(workflowDefinition.EventDefinitions.Count);
            foreach (var activityDefinition in workflowDefinition.ActivityDefinitions.Values)
            {
                foreach (var inputEventName in activityDefinition.InputEventDefinitions.Keys)
                {
                    if (eventToSubscribedActivityMap.ContainsKey(inputEventName))
                    {
                        throw new InvalidWorkflowException(
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
                throw new InvalidWorkflowException($"There are events not be subscribed: {string.Join(",", missingEvents)}");
            }

            // Find the start activity.
            // There are 2 cases how start activity can be defined. One is the start activity do not have any input event.
            // The other is the start activity depends on one single input event without publisher
            var candidateStartActivities = workflowDefinition.ActivityDefinitions.Values
                .Where(a => a.InputEventDefinitions.Count == 0)
                .ToList();

            if (candidateStartActivities.Count > 1)
            {
                throw new InvalidWorkflowException(
                    $"More than one activity without input events: {string.Join(",", candidateStartActivities.Select(e => e.Name))}.");
            }

            var candidateStartEvents = workflowDefinition.EventDefinitions.Values
                .Where(e => !workflowDefinition.ActivityDefinitions.Values.Any(a => a.OutputEventDefinitions.ContainsKey(e.Name)))
                .ToList();

            if (candidateStartEvents.Count > 1)
            {
                throw new InvalidWorkflowException(
                    $"More than one event without publisher: {string.Join(",", candidateStartEvents.Select(e => e.Name))}.");
            }

            if (candidateStartActivities.Count == 1 && candidateStartEvents.Count == 1)
            {
                throw new InvalidWorkflowException(
                    $"Both start activity {candidateStartActivities[0].Name} and start event {candidateStartEvents[0].Name} are defined.");
            }

            if (candidateStartActivities.Count == 0 && candidateStartEvents.Count == 0)
            {
                throw new InvalidWorkflowException("Neither start activity nor start event is found in the workflow.");
            }

            workflowDefinition.StartActivityDefinition = candidateStartActivities.Count == 1
                ? candidateStartActivities[0]
                : workflowDefinition.ActivityDefinitions.Values.First(a => a.InputEventDefinitions.ContainsKey(candidateStartEvents[0].Name));

            // Calculate workflow version.
            var signature = workflowDefinition.GetSignature(out bool containsLoop);
            var version = MurmurHash3.HashToString(signature);

            // Make sure the static workflow cannot have loop
            if (this.Type == WorkflowType.Static && containsLoop)
            {
                throw new InvalidWorkflowException("Static workflow must not contain loop.");
            }

            return new WorkflowDefinition
            {
                Name = workflowDefinition.Name,
                Version = version,
                Type = this.Type,
                EventDefinitions = workflowDefinition.EventDefinitions,
                ActivityDefinitions = workflowDefinition.ActivityDefinitions,
                StartActivityDefinition = workflowDefinition.StartActivityDefinition,
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
