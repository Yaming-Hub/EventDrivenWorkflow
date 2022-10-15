using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    internal sealed class EventData
    {
        public string Name { get; init; }

        public object Payload { get; init; }

        public TimeSpan DelayDuration { get; init; }
    }
}
