using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class WorkflowDefinition
    {

        public WorkflowDefinition(string name, IReadOnlyList<EventDefinition> eventDefinitions, IReadOnlyList<ActivityDefinition> activityDefinitions)
        {
            this.Name = name;
            this.EventDefinitions = eventDefinitions;
            this.ActivityDefinitions = activityDefinitions;
        }

        public string Name { get; }


        public IReadOnlyList<EventDefinition> EventDefinitions { get; }

        public IReadOnlyList<ActivityDefinition> ActivityDefinitions { get; }

    }
}
