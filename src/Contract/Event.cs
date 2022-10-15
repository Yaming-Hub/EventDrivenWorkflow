using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class Event
    {
        public string Name { get; init; }

        public TimeSpan DelayDuration { get; init; }
    }
}
