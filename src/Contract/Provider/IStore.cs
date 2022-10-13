using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Provider
{
    public interface IStore<TKey, TValue>
    {
        Task Set(TKey key, TValue value);


        Task<TValue> Get(TKey key);

        Task<IReadOnlyDictionary<TKey, TValue>> Get(IEnumerable<TKey> keys);
    }
}
