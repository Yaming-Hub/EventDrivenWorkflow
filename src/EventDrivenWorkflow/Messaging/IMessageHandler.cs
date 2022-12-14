using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Messaging
{
    public interface IMessageHandler<TMessage>
    {
        Task<MessageHandleResult> Handle(TMessage message);
    }
}
