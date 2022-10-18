using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Persistence
{
    public enum StoreErrorCode
    {
        NotFound,

        AlreadyExists,

        EtagMismatch,
    }
}
