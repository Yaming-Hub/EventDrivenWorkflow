using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventDrivenWorkflow.Runtime.Data;

namespace EventDrivenWorkflow.Utilities
{
    internal static class ResourceKeyFormat
    {
        private const string GuidPattern = "[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}";

        /// <summary>
        /// The qualified id pattern.
        /// </summary>
        private static readonly string QualifiedExecutionIdPattern = 
            @"^(\[(?<partition>[a-z][a-z0-9_\-]*)\])?" +
            $"(?<executeId>{GuidPattern}):" +
            @"(?<workflow>[a-z][a-z0-9_\-]*)" +
            $"/(?<workflowId>{GuidPattern})" +
            @"/activities/(?<activity>[a-z][a-z0-9_\-]*)" +
            $"/(?<id>{GuidPattern})$";

        private static readonly Regex QualifiedExecutionIdRegex =
            new Regex(pattern: QualifiedExecutionIdPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool TryParse(string str, out QualifiedActivityExecutionId qualifiedExecutionId)
        {
            var match = QualifiedExecutionIdRegex.Match(str);
            if (match.Success)
            {
                var partitionGroup = match.Groups["partition"];
                var partition = partitionGroup.Success ? partitionGroup.Value : null;
                var executeId = match.Groups["executeId"].Value;
                var workflow = match.Groups["workflow"].Value;
                var workflowId = match.Groups["workflowId"].Value;
                var activity = match.Groups["activity"].Value;
                var id = match.Groups["id"].Value;

                qualifiedExecutionId = new QualifiedActivityExecutionId
                {
                    PartitionKey = partition,
                    ExecutionId = Guid.Parse(executeId),
                    WorkflowName = workflow,
                    WorkflowId = Guid.Parse(workflowId),
                    ActivityName = activity,
                    ActivityId = Guid.Parse(id)
                };
                return true;
            }

            qualifiedExecutionId = null;
            return false;
        }

        public static string GetPath(this QualifiedActivityExecutionId qualifiedExecutionId)
        {
            return GetResourceKey(
                partitionKey: qualifiedExecutionId.PartitionKey,
                executionId: qualifiedExecutionId.ExecutionId,
                workflowName: qualifiedExecutionId.WorkflowName,
                workflowId: qualifiedExecutionId.WorkflowId,
                resourceType: "activities",
                resourceName: qualifiedExecutionId.ActivityName,
                resourceId: qualifiedExecutionId.ActivityId);
        }

        public static string GetActivityKey(
            string partitionKey,
            Guid executionId,
            string workflowName,
            Guid workflowId,
            string activityName)
        {
            return GetResourceKey(
                partitionKey: partitionKey,
                executionId: executionId,
                workflowName: workflowName,
                workflowId: workflowId,
                resourceType: "activities",
                resourceName: activityName,
                resourceId: null);
        }

        public static string GetEventKey(
            string partitionKey,
            Guid executionId,
            string workflowName,
            Guid workflowId,
            string eventName,
            Guid? eventId = null)
        {
            return GetResourceKey(
                partitionKey: partitionKey,
                executionId: executionId,
                workflowName: workflowName,
                workflowId: workflowId,
                resourceType: "events",
                resourceName: eventName,
                resourceId: eventId);
        }

        public static string GetWorkflowPartition(WorkflowExecutionContext workflowExecutionContext)
        {
            return GetResourceKey(
                partitionKey: workflowExecutionContext.PartitionKey,
                executionId: workflowExecutionContext.ExecutionId,
                workflowName: workflowExecutionContext.WorkflowName,
                workflowId: workflowExecutionContext.WorkflowId,
                resourceType: null,
                resourceName: null,
                resourceId: null);
        }

        private static string GetResourceKey(
            string partitionKey,
            Guid executionId,
            string workflowName,
            Guid workflowId,
            string resourceType,
            string resourceName,
            Guid? resourceId)
        {
            // [{partitionKey}]{executionId}:{workflowName}/{workflowId}/{resourceType}/{resourceName}/{resourceId}
            // For example:
            // [pk1]wf1/7fbc8f67-0577-430f-a1e5-f781bf299d20/activities/a1/dff2e5b4-e0bc-4053-8f06-48a722f8f7cd
            var length = string.IsNullOrEmpty(partitionKey) ? 0 : partitionKey.Length + 2;
            length += workflowName.Length;
            length += 39; // "/{workflowId}//"

            if (!string.IsNullOrEmpty(resourceType))
            {
                length += resourceType.Length;
                if (!string.IsNullOrEmpty(resourceName))
                {
                    length += resourceName.Length;

                    if (resourceId.HasValue)
                    {
                        length += 37; // "/{resourceId}
                    }
                }
            }

            var sb = new StringBuilder(capacity: length);
            if (!string.IsNullOrEmpty(partitionKey))
            {
                sb.Append("[").Append(partitionKey).Append("]");
            }

            sb.Append(executionId).Append(":");

            sb.Append(workflowName);
            sb.Append("/").Append(workflowId);

            if (!string.IsNullOrEmpty(resourceType))
            {
                sb.Append("/").Append(resourceType);
                if (!string.IsNullOrEmpty(resourceName))
                {
                    sb.Append("/").Append(resourceName);
                    if (resourceId.HasValue)
                    {
                        sb.Append("/").Append(resourceId.Value);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
