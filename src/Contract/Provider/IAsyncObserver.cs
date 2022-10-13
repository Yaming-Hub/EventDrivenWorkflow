using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Provider
{
    public interface IAsyncObserver<T>
    {
        Task OnComplete();

        Task OnError(Exception exception);

        Task OnNext(T item);
    }
}
