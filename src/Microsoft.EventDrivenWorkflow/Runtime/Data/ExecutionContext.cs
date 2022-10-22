// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionContext.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
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
        public QualifiedExecutionId QualifiedExecutionId => new QualifiedExecutionId
        {
            PartitionKey = this.WorkflowExecutionContext.PartitionKey,
            WorkflowName = this.WorkflowExecutionContext.WorkflowName,
            ActivityName = this.ActivityExecutionContext.ActivityName,
            ActivityExecutionId = this.ActivityExecutionContext.ActivityExecutionId
        };
    }
}
