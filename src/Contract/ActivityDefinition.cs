using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class ActivityDefinition
    {
        public string Name { get; }

        public IReadOnlyList<EventDefinition> EventsToSubscribe { get; }

        public IReadOnlyList<EventDefinition> EventsToPublish { get; }
    }
}
