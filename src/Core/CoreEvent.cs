using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class CoreEvent
    {
        public string Name { get; init; }

        public object Payload { get; init; }
    }
}
