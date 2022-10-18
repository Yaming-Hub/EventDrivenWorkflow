using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Persistence
{
    public sealed class StoreException : Exception
    {
        public StoreException(StoreErrorCode errorCode, string message)
            : this(errorCode, message, innerException: null)
        {
        }

        public StoreException(StoreErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public StoreErrorCode ErrorCode { get; }
    }
}
