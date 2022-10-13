using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class EventDefinition
    {
        public string Name { get; }

        public Type PayloadType { get; }
    }
}
