using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This class defines a workflow event. The event will be publish by activities and
    /// triggers the activity that subscribes it. The same name of event can be published
    /// by multiple activities, but can only be subscribed by one activity as downstream
    /// activity.
    /// </summary>
    public class EventDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDefinition"/> class.
        /// </summary>
        internal EventDefinition()
        {
        }

        /// <summary>
        /// Gets name of the event. The event name must be unique in a workflow.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets type of the payload. If the event doesn't have payload, then the
        /// payload type will be type of <see cref="void"/>.
        /// </summary>
        public Type PayloadType { get; init; }

        public bool HasPayload => this.PayloadType == null;
    }
}
