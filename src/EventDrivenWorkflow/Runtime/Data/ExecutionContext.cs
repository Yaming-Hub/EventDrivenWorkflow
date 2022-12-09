// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionContext.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class combines workflow execution context and activity execution context.
    /// </summary>
    public sealed class ExecutionContext
    {
        /// <summary>
        /// Gets workflow execution context.
        /// </summary>
        public WorkflowExecutionContext WorkflowExecutionContext { get; init; }

        /// <summary>
        /// Gets activity execution context.
        /// </summary>
        public ActivityExecutionContext ActivityExecutionContext { get; init; }

        /// <summary>
        /// Gets the qualified execution id. A qualified execution id can uniquely identity
        /// an activity execution.
        /// </summary>
        public QualifiedActivityExecutionId ActivityExecutionId => new QualifiedActivityExecutionId
        {
            PartitionKey = this.WorkflowExecutionContext.PartitionKey,
            ExecutionId = this.WorkflowExecutionContext.ExecutionId,
            WorkflowName = this.WorkflowExecutionContext.WorkflowName,
            ActivityName = this.ActivityExecutionContext.ActivityName,
            ActivityId = this.ActivityExecutionContext.ActivityId
        };
    }
}
