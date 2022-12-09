// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QualifiedExecutionId.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow
{
    using EventDrivenWorkflow.Runtime.Data;
    using EventDrivenWorkflow.Utilities;

    /// <summary>
    /// This class defines a qualified execution id which can uniquely identify an activity execution.
    /// </summary>
    public sealed class QualifiedActivityExecutionId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QualifiedActivityExecutionId"/> class.
        /// </summary>
        internal QualifiedActivityExecutionId()
        {
        }

        public string PartitionKey { get; init; }

        public Guid ExecutionId { get; init; }

        public string WorkflowName { get; init; }

        public Guid WorkflowId { get; init; }

        public string ActivityName { get; init; }

        public Guid ActivityId { get; init; }

        public static QualifiedActivityExecutionId FromContext(WorkflowExecutionContext wec, ActivityExecutionContext aec)
        {
            return new QualifiedActivityExecutionId
            {
                PartitionKey = wec.PartitionKey,
                ExecutionId = wec.ExecutionId,
                WorkflowName = wec.WorkflowName,
                ActivityName = aec.ActivityName,
                ActivityId = aec.ActivityId
            };
        }

        public static bool TryParse(string str, out QualifiedActivityExecutionId qualifiedExecutionId)
        {
            return ResourceKeyFormat.TryParse(str, out qualifiedExecutionId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.PartitionKey,
                this.ExecutionId,
                this.WorkflowName,
                this.WorkflowId,
                this.ActivityName,
                this.ActivityId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is QualifiedActivityExecutionId that))
            {
                return false;
            }

            return this.PartitionKey == that.PartitionKey
                && this.ExecutionId== that.ExecutionId
                && this.WorkflowName == that.WorkflowName
                && this.WorkflowId == that.WorkflowId
                && this.ActivityName == that.ActivityName
                && this.ActivityId == that.ActivityId;
        }

        public override string ToString()
        {
            return this.GetPath();
        }
    }
}
