using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    /// <summary>
    /// This interface defines a workflow activity. The activity is the atom
    /// executable operation in the workflow.
    /// </summary>
    public interface IActivity : IAsyncDisposable
    {
        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task represents the async operation.</returns>
        Task Execute(IActivityExecutionContext context, CancellationToken cancellationToken);
    }
}
