using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal sealed class WorkflowException : Exception
    {
        public WorkflowException(bool isTransient, string message)
            : base(message)
        {
            IsTransient = isTransient;
        }

        public WorkflowException(bool isTransient, string message, Exception innerException)
            : base(message, innerException)
        {
            IsTransient = isTransient;
        }

        public bool IsTransient { get; }
    }
}
