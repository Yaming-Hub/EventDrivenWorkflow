using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class Activity
    {
        public string WorkflowName { get; set; }

        public string Name { get; set; }

        public Guid WorkflowExecutionId { get; set; }

        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the list of inputs that is available.
        /// </summary>
        public List<string> AvailableInputEvents { get; set; }
    }
}
