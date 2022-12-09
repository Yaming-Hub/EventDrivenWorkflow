// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowDefinition.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This class defines the metadata of a workflow.
    /// </summary>
    public sealed class WorkflowDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowDefinition"/> class.
        /// </summary>
        internal WorkflowDefinition()
        {
        }

        /// <summary>
        /// Gets the name of the workflow.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets a guid value represents the workflow version.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets a list of events defined in the workflow.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> EventDefinitions { get; init; }

        /// <summary>
        /// Gets a list of activities defined in the workflow.
        /// </summary>
        public IReadOnlyDictionary<string, ActivityDefinition> ActivityDefinitions { get; init; }

        /// <summary>
        /// Gets the max time to execute for the workflow.
        /// </summary>
        public TimeSpan MaxExecuteDuration { get; init; }

        /// <summary>
        /// Gets trigger event of the workflow. A workflow must have one trigger event.
        /// </summary>
        public EventDefinition TriggerEvent { get; init; }

        /// <summary>
        /// Gets a list of complete events.
        /// </summary>
        public IReadOnlyList<EventDefinition> CompleteEvents { get; init; }

        /// <summary>
        /// Gets a map from event to consumer activity. One event can only be subscribed by one activity.
        /// </summary>
        public IReadOnlyDictionary<string, ActivityDefinition> EventToConsumerActivityMap { get; init; }
    }
}
