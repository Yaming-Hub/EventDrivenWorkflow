using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.IntegrationTests
{
    public sealed class Ref<T>
    {
        public T Value { get; set; }
    }
}
