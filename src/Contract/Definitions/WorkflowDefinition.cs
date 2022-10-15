using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Definitions
{
    public sealed class WorkflowDefinition
    {
        /// <summary>
        /// Gets the name of the workflow.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets a guid value represents the workflow version.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets the workflow type.
        /// </summary>
        public WorkflowType Type { get; init; }

        /// <summary>
        /// Gets a list of events defined in the workflow.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> EventDefinitions { get; init; }

        /// <summary>
        /// Gets a list of activities defined in the workflow.
        /// </summary>
        public IReadOnlyDictionary<string, ActivityDefinition> ActivityDefinitions { get; init; }

        /// <summary>
        /// Gets the max time to execute for the workflow.
        /// </summary>
        public TimeSpan MaxExecuteDuration { get; init; }

        /// <summary>
        /// Gets the initializing activity definition of the workflow.
        /// </summary>
        public ActivityDefinition InitializingActivityDefinition => this.ActivityDefinitions.Values.First(a => a.IsInitializing);

        /// <summary>
        /// Gets the terminating activity definitions of the workflow.
        /// </summary>
        public IEnumerable<ActivityDefinition> TerminatingActivityDefinitions => this.ActivityDefinitions.Values.Where(a => a.IsTerminating);
    }
}
