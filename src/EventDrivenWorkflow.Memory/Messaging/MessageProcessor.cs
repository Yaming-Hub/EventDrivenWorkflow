using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Messaging;

namespace EventDrivenWorkflow.Memory.Messaging
{
    public sealed class MessageProcessor<TMessage> : IMessageProcessor<TMessage>
    {
        private readonly object lockObject;
        private readonly List<IMessageHandler<TMessage>> handlers;
        private readonly MessageQueue<TMessage> queue;
        private readonly int maxAttemptCount;
        private readonly TimeSpan retryInterval;

        public MessageProcessor(MessageQueue<TMessage> queue, int maxAttemptCount, TimeSpan retryInterval)
        {
            this.lockObject = new object();

            this.handlers = new List<IMessageHandler<TMessage>>();
            this.queue = queue;
            this.maxAttemptCount = maxAttemptCount;
            this.retryInterval = retryInterval;
        }

        internal async Task Process(Attempt<TMessage> attempt)
        {
            List<IMessageHandler<TMessage>> copy;
            lock (this.lockObject)
            {
                copy = this.handlers.ToList();
            }

            foreach (var handler in copy)
            {
                var result = await handler.Handle(attempt.Value);
                switch (result)
                {
                    case MessageHandleResult.Complete:
                        break;

                    case MessageHandleResult.Continue:
                        continue;

                    case MessageHandleResult.Yield:
                        if (attempt.AttamptCount < maxAttemptCount)
                        {
                            var newAttempt = new Attempt<TMessage>
                            {
                                Value = attempt.Value,
                                AttamptCount = attempt.AttamptCount + 1,
                            };

                            this.queue.Enqueue(newAttempt, this.retryInterval);
                        }

                        break;
                }
            }
        }

        public void Subscribe(IMessageHandler<TMessage> handler)
        {
            lock (this.lockObject)
            {
                this.handlers.Add(handler);
            }
        }

        public void Unsubscribe(IMessageHandler<TMessage> handler)
        {
            lock (this.lockObject)
            {
                this.handlers.Remove(handler);
            }
        }
    }
}
