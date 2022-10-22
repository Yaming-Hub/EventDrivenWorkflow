using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Definitions
{
    public sealed class RetryPolicy
    {
        public static RetryPolicy DoNotRetry = new RetryPolicy
        {
            MaxRetryCount = 0,
            DelayDuration = TimeSpan.Zero
        };

        internal RetryPolicy()
        {
        }

        public int MaxRetryCount { get; init; }

        public TimeSpan DelayDuration { get; init; }
    }
}
