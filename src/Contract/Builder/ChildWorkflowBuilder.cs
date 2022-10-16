using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;

namespace Microsoft.EventDrivenWorkflow.Contract.Builder
{
    public class ChildWorkflowBuilder
    {
        private readonly ActivityBuilder activityBuilder;
        private readonly WorkflowBuilder workflowBuilder;

        internal ChildWorkflowBuilder(string name)
        {
            this.Name = name;
            this.activityBuilder = new ActivityBuilder(name);
            this.workflowBuilder = new WorkflowBuilder(name);
        }

        public string Name { get; }

        public ChildWorkflowBuilder Subscribe(string eventName)
        {
            this.activityBuilder.Subscribe(eventName);
            return this;
        }

        public ChildWorkflowBuilder Publish(string eventName)
        {
            this.activityBuilder.Publish(eventName);
            return this;
        }

        public EventBuilder RegisterEvent(string name)
        {
            return this.workflowBuilder.RegisterEvent(name);
        }

        public EventBuilder RegisterEvent<T>(string name)
        {
            return this.workflowBuilder.RegisterEvent<T>(name);
        }

        public ActivityBuilder AddActivity(string name)
        {
            return this.workflowBuilder.AddActivity(name);
        }

        public ChildWorkflowBuilder AddWorkflow(string name)
        {
            return this.workflowBuilder.AddWorkflow(name);
        }

        public WorkflowDefinition Build(string parentFullName, IReadOnlyDictionary<string, EventDefinition> parentEvents)
        {
            // Make sure input events and output events are registered as parent events
            foreach (var eventName in this.activityBuilder.InputEvents.Union(this.activityBuilder.OutputEvents))
            {
                if (!parentEvents.ContainsKey(eventName))
                {
                    throw new InvalidOperationException(
                        $"The external event {eventName} of workflow {parentFullName}.{this.Name} is not defined.");
                }
            }

            return this.workflowBuilder.Build(parentFullName, parentEvents);


            //// Make sure event name is unique across the input events, output events and internal events
            //var allEventNames = this.activityBuilder.InputEvents
            //    .Concat(this.activityBuilder.OutputEvents)
            //    .Concat(this.workflowBuilder.EventBuilders.Select(eb => eb.Name));

            //var duplicateEventNames = allEventNames
            //    .GroupBy(n => n)
            //    .Where(g => g.Count() > 1)
            //    .Select(g => g.Key)
            //    .ToList();

            //if (duplicateEventNames.Count > 0)
            //{
            //    throw new InvalidOperationException(
            //        $"There are duplicate events defined in child workflow {this.Name}: {string.Join(",", duplicateEventNames)}");
            //}

            //return this.workflowBuilder.Build();
        }

    }
}
