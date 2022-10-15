using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Messaging
{
    public enum MessageHandleResultType
    {
        Complete,
        Ignore,
        Abandon,
    }
}
