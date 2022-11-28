using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime.Data
{
    public sealed class EventReference
    {
        /// <summary>
        /// Gets id of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets name of the event.
        /// </summary>
        public string Name { get; init; }
    }
}
