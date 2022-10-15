using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Messaging
{
    public class MessageHandleResult
    {
        public MessageHandleResultType ResultType { get; init; }

        public TimeSpan Delay { get; init; }

        public static readonly MessageHandleResult Completed = new MessageHandleResult
        {
            ResultType = MessageHandleResultType.Complete,
            Delay = TimeSpan.Zero,
        };

        public static readonly MessageHandleResult Ignore = new MessageHandleResult
        {
            ResultType = MessageHandleResultType.Ignore,
            Delay = TimeSpan.Zero,
        };
    }
}
