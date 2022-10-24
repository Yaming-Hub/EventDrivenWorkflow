using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Persistence
{
    public interface IEntityStore<TEntity> where TEntity : IEntity
    {
        Task<TEntity> Get(string partitionKey, string key);

        Task<TEntity> GetOrAdd(string partitionKey, string key, Func<TEntity> getEntity);

        Task<IEnumerable<TEntity>> GetMany(string partitionKey, IEnumerable<string> keys);

        Task<IReadOnlyList<TEntity>> List(string partitionKey);

        Task Upsert(string partitionKey, string key, TEntity entity);

        Task Update(string partitionKey, string key, TEntity entity);

        Task Delete(string partitionKey, string key);
    }
}
