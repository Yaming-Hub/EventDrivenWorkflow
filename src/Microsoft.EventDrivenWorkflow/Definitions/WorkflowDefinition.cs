// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowDefinition.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Definitions
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
        /// Gets type of the workflow.
        /// </summary>
        public WorkflowType Type { get; init; }

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
        /// Gets the initializing activity definition of the workflow.
        /// </summary>
        public ActivityDefinition StartActivityDefinition { get; internal set; }

        /// <summary>
        /// Gets a map from event to subscribed activity.
        /// </summary>
        public IReadOnlyDictionary<string, ActivityDefinition> EventToSubscribedActivityMap { get; init; }
    }
}
