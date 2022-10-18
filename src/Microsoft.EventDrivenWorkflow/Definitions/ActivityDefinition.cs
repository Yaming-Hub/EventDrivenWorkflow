using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Definitions
{
    public sealed class ActivityDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityDefinition"/> class.
        /// </summary>
        internal ActivityDefinition()
        {
        }

        /// <summary>
        /// Gets the name of activity definition.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets a value indicates whether the activity is an async activity.
        /// </summary>
        public bool IsAsync { get; init; }

        /// <summary>
        /// Gets a list of input event definitions. An activity will only be triggered 
        /// if all defined input events are received.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> InputEventDefinitions { get; init; }

        /// <summary>
        /// Gets a list of output event definitions. An activity may publish any number
        /// number of events, the events must be defined in this list.
        /// </summary>
        public IReadOnlyDictionary<string, EventDefinition> OutputEventDefinitions { get; init; }

        /// <summary>
        /// Gets the max time to execute for the activity.
        /// </summary>
        public TimeSpan MaxExecuteDuration { get; init; }

        /// <summary>
        /// Gets a value indicates whether the activity is an initializing activity. One
        /// workflow has only one initializing activity.
        /// </summary>
        public bool IsInitializing => InputEventDefinitions.Count == 0;

        /// <summary>
        /// Gets a value indicates whether the activity is an terminating activity. One
        /// workflow will have one or more terminating activities.
        /// </summary>
        public bool IsTerminating => OutputEventDefinitions.Count == 0;
    }
}
