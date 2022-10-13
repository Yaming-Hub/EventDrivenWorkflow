using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public sealed class ActivityContext
    {
        public Guid ActivityId { get; }

        public ActivityDefinition ActivityDefinition { get; }

        public T GetEventPayload<T>(string eventName)
        {
            throw new NotImplementedException();
        }

        public void PublishEvents(params Event[] events)
        {
            throw new NotImplementedException();
        }
    }
}
