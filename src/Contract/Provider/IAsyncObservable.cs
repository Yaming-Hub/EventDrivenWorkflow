using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Provider
{
    public interface IAsyncObservable<T>
    {
        void Subscribe(IAsyncObserver<T> observer);

        void Unsubscribe(IAsyncObserver<T> observer);
    }
}
