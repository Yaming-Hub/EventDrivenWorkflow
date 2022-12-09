using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime.Data
{
    public class ExecuteActivityControlModel : ControlModel
    {
        /// <summary>
        /// Gets the name of the target activity.
        /// </summary>
        public string TargetActivityName { get; init; }

        public QualifiedExecutionContext ExecutionContext { get; init; }

        /// <summary>
        /// Gets the event model.
        /// </summary>
        public EventModel Event { get; init; }
    }
}
