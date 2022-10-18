using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow
{
    public sealed class Event<T> : Event
    {
        public T Payload { get; init; }
    }
}
