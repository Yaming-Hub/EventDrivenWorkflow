using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Persistence
{
    public interface IEntity
    {
        string ETag { get; set; }

        DateTime ExpireDateTime { get; init; }
    }
}
