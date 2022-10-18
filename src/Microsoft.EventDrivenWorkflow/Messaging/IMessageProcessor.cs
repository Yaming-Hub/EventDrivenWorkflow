using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Messaging
{
    public interface IMessageProcessor<TMessage>
    {
        void Subscribe(IMessageHandler<TMessage> handler);

        void Unsubscribe(IMessageHandler<TMessage> handler);
    }
}
