using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime.Data;

namespace Microsoft.EventDrivenWorkflow
{
    public sealed class QualifiedExecutionId
    {
        /// <summary>
        /// The qualified id pattern.
        /// </summary>
        private const string Pattern = @"^(\[(?<partition>[a-z][a-z0-9_\-]*)\])?" +
            @"(?<workflow>[a-z][a-z0-9_\-]*)" +
            @"/activities/(?<activity>[a-z][a-z0-9_\-]*)" +
            @"/(?<id>[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12})$";

        /// <summary>
        /// The qualified execution id regex.
        /// </summary>
        private static readonly Regex Regex = new Regex(pattern: Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal QualifiedExecutionId()
        {
        }

        public string PartitionKey { get; init; }

        public string WorkflowName { get; init; }

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
            var match = QualifiedExecutionId.Regex.Match(str);
            if (match.Success)
            {
                var partitionGroup = match.Groups["partition"];
                var partition = partitionGroup.Success ? partitionGroup.Value : null;
                var workflow = match.Groups["workflow"].Value;
                var activity = match.Groups["activity"].Value;
                var id = match.Groups["id"].Value;

                qualifiedExecutionId = new QualifiedExecutionId
                {
                    PartitionKey = partition,
                    WorkflowName = workflow,
                    ActivityName = activity,
                    ActivityExecutionId = Guid.Parse(id)
                };
                return true;
            }

            qualifiedExecutionId = null;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.PartitionKey, this.WorkflowName, this.ActivityName, this.ActivityExecutionId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is QualifiedExecutionId that))
            {
                return false;
            }

            return this.PartitionKey == that.PartitionKey
                && this.WorkflowName == that.WorkflowName
                && this.ActivityName == that.ActivityName
                && this.ActivityExecutionId == that.ActivityExecutionId;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(128);
            if (!string.IsNullOrEmpty(this.PartitionKey))
            {
                sb.Append("[").Append(this.PartitionKey).Append("]");
            }

            sb.Append(this.WorkflowName);
            sb.Append("/activities/").Append(this.ActivityName);
            sb.Append("/").Append(this.ActivityExecutionId);

            return sb.ToString();
        }
    }
}
