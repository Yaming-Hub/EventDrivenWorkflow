using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Definitions
{
    public static class WorkflowDefinitionExtensions
    {
        public static void Traverse(this WorkflowDefinition workflowDefinition, Action<Link> visit, bool sort = false)
        {
            HashSet<Link> visited = new HashSet<Link>();
            var queue = new Queue<Link>();
            queue.Enqueue(new Link
            {
                Source = null,
                Event = null,
                Target = workflowDefinition.InitializingActivityDefinition
            });

            while (queue.Count > 0)
            {
                var link = queue.Dequeue();

                visit(link);
                visited.Add(link);

                var outputEventDefinitions = sort
                    ? link.Target.OutputEventDefinitions.Values.OrderBy(a => a.Name)
                    : link.Target.OutputEventDefinitions.Values;

                foreach (var outputEventDefinition in outputEventDefinitions)
                {
                    // An event is subscribed by one and only one activity.
                    var outputActivity = workflowDefinition.ActivityDefinitions.Values
                        .First(a => a.InputEventDefinitions.ContainsKey(outputEventDefinition.Name));

                    var outputLink = (new Link
                    {
                        Source = link.Target,
                        Event = outputEventDefinition,
                        Target = outputActivity
                    });

                    if (!visited.Contains(outputLink))
                    {
                        queue.Enqueue(outputLink);
                    }
                }
            }
        }

        public static string GetSignature(this WorkflowDefinition workflowDefinition)
        {
            var sb = new StringBuilder(1024);
            workflowDefinition.Traverse(link =>
            {
                if (link.Source != null)
                {
                    sb.Append(link.Source.Name).Append("/");

                    if (link.Event.PayloadType != null)
                    {
                        sb.Append(link.Event.Name).Append("(").Append(link.Event.PayloadType.FullName).Append(")");
                    }
                    else
                    {
                        sb.Append(link.Event.Name);
                    }

                    sb.Append("/").Append(link.Target.Name).Append(",");
                }
                else
                {
                    // The initializating activity doesn't have source.
                    sb.Append(link.Target.Name).Append(",");
                }
            });

            sb.Length--; // Trim last ","
            return sb.ToString();
        }

        public class Link
        {
            public ActivityDefinition Source { get; init; }

            public EventDefinition Event { get; init; }

            public ActivityDefinition Target { get; init; }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Source, this.Event, this.Target);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Link that))
                {
                    return false;
                }

                return this.Source == that.Source && this.Event == that.Event && this.Target == that.Target;
            }
        }
    }
}
