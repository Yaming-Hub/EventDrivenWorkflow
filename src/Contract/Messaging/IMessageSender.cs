using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Messaging
{
    public interface IMessageSender<TMessage>
    {
        Task Send(TMessage message);

        Task Send(TMessage message, TimeSpan delay);
    }
}
