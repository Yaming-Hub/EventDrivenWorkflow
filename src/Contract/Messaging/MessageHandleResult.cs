using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Messaging
{
    public enum MessageHandleResult
    {
        /// <summary>
        /// The message has been handled completely and should not be further processed.
        /// </summary>
        Complete,

        /// <summary>
        /// The message has been handled completely but should continue to be processed by other handler.
        /// </summary>
        Continue,

        /// <summary>
        /// The message cannot be handled at this moment, should be retried later.
        /// </summary>
        Yield,
    }
}
