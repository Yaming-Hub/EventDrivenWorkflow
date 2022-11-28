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
    public sealed class QualifiedExecutionId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QualifiedExecutionId"/> class.
        /// </summary>
        internal QualifiedExecutionId()
        {
        }

        public string PartitionKey { get; init; }

        public string WorkflowName { get; init; }

        public Guid WorkflowId { get; init; }

        public string ActivityName { get; init; }

        public Guid ActivityExecutionId { get; init; }

        public static QualifiedExecutionId FromContext(WorkflowExecutionContext wec, ActivityExecutionContext aec)
        {
            return new QualifiedExecutionId
            {
                PartitionKey = wec.PartitionKey,
                WorkflowName = wec.WorkflowName,
                ActivityName = aec.ActivityName,
                ActivityExecutionId = aec.ActivityExecutionId
            };
        }

        public static bool TryParse(string str, out QualifiedExecutionId qualifiedExecutionId)
        {
            return ResourceKeyFormat.TryParse(str, out qualifiedExecutionId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.PartitionKey,
                this.WorkflowName,
                this.WorkflowId,
                this.ActivityName,
                this.ActivityExecutionId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is QualifiedExecutionId that))
            {
                return false;
            }

            return this.PartitionKey == that.PartitionKey
                && this.WorkflowName == that.WorkflowName
                && this.WorkflowId == that.WorkflowId
                && this.ActivityName == that.ActivityName
                && this.ActivityExecutionId == that.ActivityExecutionId;
        }

        public override string ToString()
        {
            return this.GetPath();
        }
    }
}
