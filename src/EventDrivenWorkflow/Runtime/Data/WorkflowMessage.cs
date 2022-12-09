using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime.Data
{
    public abstract class WorkflowMessage
    {
        public string WorkflowName { get; init; }
    }
}
