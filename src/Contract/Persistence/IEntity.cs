using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Persistence
{
    public interface IEntity
    {
        string PartitionKey { get; init; }

        string PrimaryKey { get; init; }

        string ETag { get; init; }
    }
}
