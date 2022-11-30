// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowDefinitionExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    using System.Text;

    /// <summary>
    /// This class defines extension methods of <see cref="WorkflowDefinition"/> class.
    /// </summary>
    public static class WorkflowDefinitionExtensions
    {
        /// <summary>
        /// Traverse the workflow graph.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <param name="visit">A callback method to be invoked against each workflow link.</param>
        /// <param name="sort">Whether the to sort output event to get deterministic visiting order.</param>
        public static void Traverse(
            this WorkflowDefinition workflowDefinition,
            Action<WorkflowLink> visit,
            Action<WorkflowLink> visitDuplicate,
            bool sort = false)
        {
            Traverse(
                triggerEvent: workflowDefinition.TriggerEvent,
                eventToConsumerActivityMap: workflowDefinition.EventToConsumerActivityMap,
                visit: visit,
                visitDuplicate: visitDuplicate,
                sort: sort);
        }
        /// <summary>
        /// Gets the signature of the workflow.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <param name="containsLoop">Returns whether the workflow contain loop.</param>
        /// <returns>The workflow signature.</returns>
        internal static string GetSignature(this WorkflowDefinition workflowDefinition, out bool containsLoop)
        {
            return GetSignature(
                triggerEvent: workflowDefinition.TriggerEvent,
                eventToConsumerActivityMap: workflowDefinition.EventToConsumerActivityMap,
                containsLoop: out containsLoop);
        }

        /// <summary>
        /// Traverse the workflow graph.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <param name="visit">A callback method to be invoked against each workflow link.</param>
        /// <param name="sort">Whether the to sort output event to get deterministic visiting order.</param>
        internal static void Traverse(
            EventDefinition triggerEvent,
            IReadOnlyDictionary<string, ActivityDefinition> eventToConsumerActivityMap,
            Action<WorkflowLink> visit,
            Action<WorkflowLink> visitDuplicate,
            bool sort = false)
        {
            HashSet<WorkflowLink> visited = new HashSet<WorkflowLink>();

            var queue = new Queue<WorkflowLink>();

            // Queue the inlet link.
            queue.Enqueue(new WorkflowLink
            {
                Source = null,
                Event = triggerEvent,
                Target = eventToConsumerActivityMap[triggerEvent.Name]
            });

            while (queue.Count > 0)
            {
                var link = queue.Dequeue();

                visit?.Invoke(link);
                visited.Add(link);

                // The is the outlet link.
                if (link.Target == null)
                {
                    continue;
                }

                var outputEventDefinitions = sort
                    ? link.Target.OutputEventDefinitions.Values.OrderBy(a => a.Name)
                    : link.Target.OutputEventDefinitions.Values;

                foreach (var outputEventDefinition in outputEventDefinitions)
                {
                    // An event is subscribed one activity
                    if (!eventToConsumerActivityMap.TryGetValue(outputEventDefinition.Name, out var outputActivity))
                    {
                        // The complete event may not be subscribed by any activity.
                        outputActivity = null;
                    }

                    var outputLink = new WorkflowLink
                    {
                        Source = link.Target,
                        Event = outputEventDefinition,
                        Target = outputActivity
                    };

                    if (!visited.Contains(outputLink))
                    {
                        queue.Enqueue(outputLink);
                    }
                    else
                    {
                        visitDuplicate?.Invoke(link);
                    }
                }
            }
        }


        /// <summary>
        /// Gets string contains name and version of the workflow definition.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <returns>A string contains name and version of the workflow definition.</returns>
        internal static string GetNameAndVersion(this WorkflowDefinition workflowDefinition)
            => $"{workflowDefinition.Name}(ver:{workflowDefinition.Version}";

        /// <summary>
        /// Gets the signature of the workflow.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <param name="containsLoop">Returns whether the workflow contain loop.</param>
        /// <returns>The workflow signature.</returns>
        internal static string GetSignature(EventDefinition triggerEvent,
            IReadOnlyDictionary<string, ActivityDefinition> eventToConsumerActivityMap,
            out bool containsLoop)
        {
            var sb = new StringBuilder(1024);
            int duplicateLinkCount = 0;

            Traverse(
                triggerEvent,
                eventToConsumerActivityMap,
                visit: link =>
                {
                    if (link.Source != null)
                    {
                        sb.Append(link.Source.Name);
                    }

                    sb.Append("/");
                    if (link.Event.PayloadType != null)
                    {
                        sb.Append(link.Event.Name).Append("(").Append(link.Event.PayloadType.FullName).Append(")");
                    }
                    else
                    {
                        sb.Append(link.Event.Name);
                    }

                    sb.Append("/");
                    if (link.Target != null)
                    {
                        sb.Append(link.Target.Name);
                    }

                    sb.Append(",");
                },
                visitDuplicate: link => { duplicateLinkCount++; });

            containsLoop = duplicateLinkCount > 0;

            sb.Length--; // Trim last ","
            return sb.ToString();
        }
    }
}
