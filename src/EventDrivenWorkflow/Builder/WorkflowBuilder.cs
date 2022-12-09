// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowBuilder.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Builder
{
    using System;
    using System.Linq;
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
        public WorkflowBuilder(string name)
        {
            if (StringConstraint.Name.IsValid(name, out string reason))
            {
                throw new ArgumentException($"Workflow name {reason}", paramName: nameof(name));
            }

            this.Name = name;
            this.eventBuilders = new List<EventBuilder>();
            this.activityBuilders = new List<ActivityBuilder>();
            this.childWorkflowBuilders = new List<WorkflowBuilder>();
            this.maxExecuteDuration = TimeSpan.FromDays(1);
        }

        internal string Name { get; }

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

        // TODO: Remove.
        public WorkflowBuilder AddWorkflow(string name)
        {
            this.EnsureActivityNameIsUnique(name);

            var childWorkflowBuilder = new WorkflowBuilder(name);
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
            // TODO: Workflow must have a trigger event.
            // TODO: Workflow may have an optional complete event.

            var workflowDefinition = this.Build(null, new Dictionary<string, EventDefinition>());
            Dictionary<string, ActivityDefinition> activityDefinitions = new Dictionary<string, ActivityDefinition>(workflowDefinition.ActivityDefinitions);

            if (activityDefinitions.Count == 0)
            {
                throw new InvalidWorkflowException($"There is no activity defined in workflow {this.Name}.");
            }

            // Build event to consumer activity and event to producer activities map.
            var eventToConsumerActivityMap = new Dictionary<string, ActivityDefinition>(workflowDefinition.EventDefinitions.Count);
            var eventToProducerActivitiesMap = new Dictionary<string, List<ActivityDefinition>>(workflowDefinition.EventDefinitions.Count);
            foreach (var activityDefinition in activityDefinitions.Values)
            {
                if (activityDefinition.InputEventDefinitions.Count == 0)
                {
                    throw new InvalidWorkflowException($"There is no input event defined for activity {activityDefinition.Name}.");
                }

                foreach (var inputEventName in activityDefinition.InputEventDefinitions.Keys)
                {
                    if (!workflowDefinition.EventDefinitions.ContainsKey(inputEventName))
                    {
                        throw new InvalidWorkflowException($"Input event {inputEventName} of activity {activityDefinition.Name} is not registered.");
                    }

                    // Check there is no more than one activity subscribe to the same event.
                    if (eventToConsumerActivityMap.ContainsKey(inputEventName))
                    {
                        throw new InvalidWorkflowException(
                            $"The event {inputEventName} is subscribed by both " +
                            $"{eventToConsumerActivityMap[inputEventName].Name} activity " +
                            $" and {activityDefinition.Name} activity.");
                    }

                    eventToConsumerActivityMap.Add(inputEventName, activityDefinition);
                }

                foreach (var outputEventName in activityDefinition.OutputEventDefinitions.Keys)
                {
                    if (!workflowDefinition.EventDefinitions.ContainsKey(outputEventName))
                    {
                        throw new InvalidWorkflowException($"Output event {outputEventName} of activity {activityDefinition.Name} is not registered.");
                    }

                    if (!eventToProducerActivitiesMap.TryGetValue(outputEventName, out var producerActivities))
                    {
                        producerActivities = new List<ActivityDefinition>();
                        eventToProducerActivitiesMap.Add(outputEventName, producerActivities);
                    }

                    producerActivities.Add(activityDefinition);
                }
            }

            // Find trigger event. Trigger event is cannot be produced by any activity.
            var triggerEvents = workflowDefinition.EventDefinitions.Values
                .Where(e => !eventToProducerActivitiesMap.ContainsKey(e.Name))
                .ToList();

            if (triggerEvents.Count == 0)
            {
                throw new InvalidWorkflowException("There is no trigger event found in the workflow definition.");
            }

            if (triggerEvents.Count > 1)
            {
                throw new InvalidWorkflowException("There are more than one trigger events found in the workflow definition. "
                    + $"Trigger events are {string.Join(",", triggerEvents)}.");
            }

            var triggerEvent = triggerEvents[0];

            // Find the optional complete event.
            List<EventDefinition> completeEvents = workflowDefinition.EventDefinitions.Values
                .Where(e => !eventToConsumerActivityMap.ContainsKey(e.Name))
                .ToList();

            // Create a virtual complete activity which subscribes to all complete events
            // Add the complete event to the workflow graph.
            if (completeEvents.Count > 0)
            {
                var completeActivity = new ActivityDefinition
                {
                    Name = ActivityDefinition.CompleteActivityName,
                    InputEventDefinitions = completeEvents.ToDictionary(x => x.Name, x => x),
                    OutputEventDefinitions = new Dictionary<string, EventDefinition>(),
                    IsAsync = false,
                    MaxExecuteDuration = TimeSpan.FromSeconds(30),
                    RetryPolicy = RetryPolicy.DoNotRetry,
                };

                activityDefinitions.Add(completeActivity.Name, completeActivity);
                foreach(var completeEvent in completeEvents)
                {
                    eventToConsumerActivityMap.Add(completeEvent.Name, completeActivity);
                }
            }

            // Find the start activity.
            // There are 2 cases how start activity can be defined. One is the start activity do not have any input event.
            // The other is the start activity depends on one single input event without publisher
            var candidateStartActivities = activityDefinitions.Values
                .Where(a => a.InputEventDefinitions.Count == 0)
                .ToList();

            if (candidateStartActivities.Count > 1)
            {
                throw new InvalidWorkflowException(
                    $"More than one activity without input events: {string.Join(",", candidateStartActivities.Select(e => e.Name))}.");
            }

            // Calculate workflow version.
            var signature = WorkflowDefinitionExtensions.GetSignature(triggerEvent, eventToConsumerActivityMap, out bool containsLoop);
            var version = MurmurHash3.HashToString(signature);

            return new WorkflowDefinition
            {
                Name = workflowDefinition.Name,
                Version = version,
                EventDefinitions = workflowDefinition.EventDefinitions,
                ActivityDefinitions = activityDefinitions,
                TriggerEvent = triggerEvents[0],
                CompleteEvents = completeEvents,
                MaxExecuteDuration = this.maxExecuteDuration,
                EventToConsumerActivityMap = eventToConsumerActivityMap,
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
