using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Persistence;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.Persistence
{
    public class EventEntity : IEntity
    {
        public string PartitionKey { get; init; }

        public string PrimaryKey { get; init; }

        public string ETag { get; init; }

        public Event EventData { get; init; }
    }
}
