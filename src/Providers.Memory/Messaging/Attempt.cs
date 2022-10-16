using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Providers.Memory.Messaging
{
    internal class Attempt<T>
    {
        public T Value { get; init; }

        public int AttamptCount { get; init; }
    }
}
