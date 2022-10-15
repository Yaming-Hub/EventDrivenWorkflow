using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Persistence
{
    public interface IStore<TEntity> where TEntity : IEntity
    {
        Task<TEntity> Get(string partitionKey, string key);

        Task<IReadOnlyDictionary<string, TEntity>> GetMany(string partitionKey, IEnumerable<string> keys);

        Task Upsert(TEntity value);

        Task AddOrUpdate(TEntity value);
    }
}
