// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityDefinition.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This class defines metadata of a workflow activity.
    /// </summary>
    public sealed class ActivityDefinition
    {
        /// <summary>
        /// The virtual complete activity name.
        /// </summary>
        internal const string CompleteActivityName = "Complete-df4b46df-829b-4708-ba00-ceb42a4cfa73";

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityDefinition"/> class.
        /// </summary>
        internal ActivityDefinition()
        {
        }

        /// <summary>
        /// Gets the name of activity definition.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets a value indicates whether the activity is an async activity.
        /// </summary>
        public bool IsAsync { get; init; }

        /// <summary>
        /// Gets the retry policy.
        /// </summary>
        public RetryPolicy RetryPolicy { get; init; } 

        /// <summary>
        /// Gets a list of input event definitions. An activity will only be triggered 
        /// if all defined input events are received.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> InputEventDefinitions { get; init; }

        /// <summary>
        /// Gets a list of output event definitions. An activity may publish any number
        /// number of events, the events must be defined in this list.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> OutputEventDefinitions { get; init; }

        /// <summary>
        /// Gets the max time to execute for the activity.
        /// </summary>
        public TimeSpan MaxExecuteDuration { get; init; }

        /// <summary>
        /// Gets a bool value indicates whether the activity is the auto-generated complete activity.
        /// </summary>
        public bool IsCompleteActivity => this.Name == CompleteActivityName;
    }
}
