﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Persistence
{
    public sealed class StoreException : Exception
    {
        public StoreException(StoreErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public StoreErrorCode ErrorCode { get; }
    }
}
