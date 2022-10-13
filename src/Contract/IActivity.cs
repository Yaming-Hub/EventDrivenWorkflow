using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public interface IActivity
    {
        Task Execute(ActivityContext context, CancellationToken cancellationToken);
    }
}
