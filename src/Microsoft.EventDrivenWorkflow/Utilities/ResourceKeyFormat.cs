﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Utilities
{
    internal static class ResourceKeyFormat
    {
        private const string GuidPattern = "[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}";

        /// <summary>
        /// The qualified id pattern.
        /// </summary>
        private static readonly string QualifiedExecutionIdPattern = @"^(\[(?<partition>[a-z][a-z0-9_\-]*)\])?" +
            @"(?<workflow>[a-z][a-z0-9_\-]*)" +
            $"/(?<workflowId>{GuidPattern})" +
            @"/activities/(?<activity>[a-z][a-z0-9_\-]*)" +
            $"/(?<id>{GuidPattern})$";

        private static readonly Regex QualifiedExecutionIdRegex =
            new Regex(pattern: QualifiedExecutionIdPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool TryParse(string str, out QualifiedExecutionId qualifiedExecutionId)
        {
            var match = QualifiedExecutionIdRegex.Match(str);
            if (match.Success)
            {
                var partitionGroup = match.Groups["partition"];
                var partition = partitionGroup.Success ? partitionGroup.Value : null;
                var workflow = match.Groups["workflow"].Value;
                var workflowId = match.Groups["workflowId"].Value;
                var activity = match.Groups["activity"].Value;
                var id = match.Groups["id"].Value;

                qualifiedExecutionId = new QualifiedExecutionId
                {
                    PartitionKey = partition,
                    WorkflowName = workflow,
                    WorkflowId = Guid.Parse(workflowId),
                    ActivityName = activity,
                    ActivityExecutionId = Guid.Parse(id)
                };
                return true;
            }

            qualifiedExecutionId = null;
            return false;
        }

        public static string GetPath(this QualifiedExecutionId qualifiedExecutionId)
        {
            return GetResourceKey(
                partitionKey: qualifiedExecutionId.PartitionKey,
                workflowName: qualifiedExecutionId.WorkflowName,
                workflowId: qualifiedExecutionId.WorkflowId,
                resourceType: "activities",
                resourceName: qualifiedExecutionId.ActivityName,
                resourceId: qualifiedExecutionId.ActivityExecutionId);
        }

        public static string GetActivityKey(
            string partitionKey,
            string workflowName,
            Guid workflowId,
            string activityName)
        {
            return GetResourceKey(
                partitionKey: partitionKey,
                workflowName: workflowName,
                workflowId: workflowId,
                resourceType: "activities",
                resourceName: activityName,
                resourceId: null);
        }

        public static string GetEventKey(
            string partitionKey,
            string workflowName,
            Guid workflowId,
            string eventName)
        {
            return GetResourceKey(
                partitionKey: partitionKey,
                workflowName: workflowName,
                workflowId: workflowId,
                resourceType: "events",
                resourceName: eventName,
                resourceId: null);
        }

        private static string GetResourceKey(
            string partitionKey,
            string workflowName,
            Guid workflowId,
            string resourceType,
            string resourceName,
            Guid? resourceId)
        {
            // [{partitionKey}]{workflowName}/{workflowId}/{resourceType}/{resourceName}/{resourceId}
            // For example:
            // [pk1]wf1/7fbc8f67-0577-430f-a1e5-f781bf299d20/activities/a1/dff2e5b4-e0bc-4053-8f06-48a722f8f7cd
            var length = string.IsNullOrEmpty(partitionKey) ? 0 : partitionKey.Length + 2;
            length += workflowName.Length;
            length += resourceType.Length;
            length += resourceName.Length;
            length += 39; // "/{workflowId}//"
            if (resourceId.HasValue)
            {
                length += 37; // "/{resourceId}
            }

            var sb = new StringBuilder(capacity: length);
            if (!string.IsNullOrEmpty(partitionKey))
            {
                sb.Append("[").Append(partitionKey).Append("]");
            }

            sb.Append(workflowName);
            sb.Append("/").Append(workflowId);
            sb.Append("/").Append(resourceType);
            sb.Append("/").Append(resourceName);
            if (resourceId.HasValue)
            {
                sb.Append("/").Append(resourceId.Value);
            }

            return sb.ToString();
        }
    }
}
