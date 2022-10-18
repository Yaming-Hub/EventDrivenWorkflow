using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow
{
    public interface IAsyncActivityCompleter
    {
        Task EndExecute(ActivityExecutionInfo activityExecutionInfo, params Event[] outputEvents);
    }
}
