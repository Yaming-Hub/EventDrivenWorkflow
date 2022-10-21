using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public string PartitionKey { get; init; }

        public string WorkflowName { get; init; }

        public string ActivityName { get; init; }

        public Guid ExecutionId { get; init; }

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
                    ExecutionId = Guid.Parse(id)
                };
                return true;
            }

            qualifiedExecutionId = null;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.PartitionKey, this.WorkflowName, this.ActivityName, this.ExecutionId);
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
                && this.ExecutionId == that.ExecutionId;
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
            sb.Append("/").Append(this.ExecutionId);

            return sb.ToString();
        }
    }
}
