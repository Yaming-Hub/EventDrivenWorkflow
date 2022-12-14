// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityBuilder.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Builder
{
    using EventDrivenWorkflow.Definitions;

    /// <summary>
    /// This class builds workflow activity.
    /// </summary>
    public sealed class ActivityBuilder
    {
        private readonly bool isAsync;
        private readonly List<string> inputEvents;
        private readonly List<string> outputEvents;

        private TimeSpan maxExecutionDuration;
        private RetryPolicy retryPolicy;

        internal ActivityBuilder(string name, bool isAsync)
        {
            if (StringConstraint.Name.IsValid(name, out string reason))
            {
                throw new ArgumentException($"Activity name {reason}", paramName: nameof(name));
            }

            this.Name = name;
            this.isAsync = isAsync;

            this.inputEvents = new List<string>();
            this.outputEvents = new List<string>();
            this.maxExecutionDuration = TimeSpan.Zero; // unlimited
            this.retryPolicy = RetryPolicy.DoNotRetry;
        }

        internal string Name { get; }

        internal IReadOnlyList<string> InputEvents => this.inputEvents;

        internal IReadOnlyList<string> OutputEvents => this.outputEvents;


        public ActivityBuilder Subscribe(string eventName)
        {
            AddEvent(eventName, "input", this.inputEvents);
            return this;
        }

        public ActivityBuilder Publish(string eventName)
        {
            AddEvent(eventName, "output", this.outputEvents);
            return this;
        }

        public ActivityBuilder Retry(int maxRetryCount, TimeSpan delayDuration)
        {
            if (maxRetryCount < 0)
            {
                throw new ArgumentOutOfRangeException("The max retry count must be greater than or equal to zero.");
            }

            if (delayDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("The delay duration must be greater than or equal to zero.");
            }

            this.retryPolicy = new RetryPolicy
            {
                MaxRetryCount = maxRetryCount,
                DelayDuration = delayDuration
            };

            return this;
        }

        internal ActivityDefinition Build(string parentFullName, IReadOnlyDictionary<string, EventDefinition> events)
        {
            return new ActivityDefinition
            {
                Name = parentFullName == null ? this.Name : $"{parentFullName}.{this.Name}",
                InputEventDefinitions = this.ResolveEvent("input", this.inputEvents, events),
                OutputEventDefinitions = this.ResolveEvent("output", this.outputEvents, events),
                MaxExecuteDuration = this.maxExecutionDuration,
                IsAsync = this.isAsync,
                RetryPolicy = this.retryPolicy,
            };
        }

        private IReadOnlyDictionary<string, EventDefinition> ResolveEvent(string listName, List<string> list, IReadOnlyDictionary<string, EventDefinition> events)
        {
            var dictionary = new Dictionary<string, EventDefinition>(capacity: list.Count);
            foreach (var eventName in list)
            {
                if (!events.TryGetValue(eventName, out var eventDefinition))
                {
                    throw new InvalidOperationException(
                        $"The {listName} event {eventName} of activity {this.Name} is not defined.");
                }

                dictionary[eventName] = eventDefinition;
            }

            return dictionary;
        }

        private static void AddEvent(string eventName, string listName, List<string> list)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            if (list.Contains(eventName))
            {
                throw new InvalidOperationException($"The {listName} event {eventName} already exists.");
            }

            list.Add(eventName);
        }
    }
}
