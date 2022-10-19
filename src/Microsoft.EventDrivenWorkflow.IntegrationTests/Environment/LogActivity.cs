using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Runtime;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class LogActivity : IActivity
    {
        public Task Execute(
            ActivityExecutionContext context,
            IEventRetriever eventRetriever,
            IEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine("Execute context.GetExecutionPath()");
            return Task.CompletedTask;
        }
    }
}
