using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    internal class Event
    {
        public Guid Id { get; set; }

        public string Key { get; set; }

        public string Name { get; init; }

        public object Payload { get; init; }

        public TimeSpan Delay { get; init; }
    }
}
