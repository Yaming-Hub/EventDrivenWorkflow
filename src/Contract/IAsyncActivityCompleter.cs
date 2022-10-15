using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public interface IAsyncActivityCompleter
    {
        Task Complete(ActivityExecutionIdentity activityExecutionId, IEnumerable<Event> outputEvents);
    }
}
