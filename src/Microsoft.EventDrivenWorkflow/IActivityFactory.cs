using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow
{
    /// <summary>
    /// This interface defines a factory which creates activity by name. Each
    /// workflow will have it's own activity factory.
    /// </summary>
    public interface IActivityFactory
    {
        IActivity CreateActivity(string partitionKey, string name);

        IAsyncActivity CreateAsyncActivity(string partitionKey, string name);
    }
}
