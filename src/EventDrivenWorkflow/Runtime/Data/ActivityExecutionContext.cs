// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityExecutionContext.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the context information of the executing activity.
    /// </summary>
    public sealed class ActivityExecutionContext
    {
        /// <summary>
        /// Gets the activity name.
        /// </summary>
        public string ActivityName { get; init; }

        /// <summary>
        /// Gets the execution id of the activity.
        /// </summary>
        public Guid ActivityId { get; init; }

        /// <summary>
        /// Gets the start time of the execution.
        /// </summary>
        public DateTime ActivityExecutionStartDateTime { get; init; }

        public EventReference TriggerEventReference { get; init; }


        public int AttemptCount { get; init; }

        ///// <summary>
        ///// Gets the qualified execution id. A qualified execution id can uniquely identity
        ///// an activity execution.
        ///// </summary>
        //public QualifiedExecutionId QualifiedExecutionId => new QualifiedExecutionId
        //{
        //    PartitionKey = PartitionKey,
        //    WorkflowName = WorkflowName,
        //    ActivityName = ActivityName,
        //    ExecutionId = ExecutionId
        //};
    }
}
