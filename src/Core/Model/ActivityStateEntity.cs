using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Persistence;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class ActivityStateEntity : IEntity
    {
        public string WorkflowName { get; init; }

        public string WorkflowVersion { get; init; }

        public string Name { get; init; }

        public Guid WorkflowId { get; init; }

        /// <summary>
        /// Gets or sets the list of inputs that is available.
        /// </summary>
        public List<string> AvailableInputEvents { get; init; }


        public string ETag { get; init; }
    }
}
