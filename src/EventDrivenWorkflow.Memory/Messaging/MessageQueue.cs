using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Messaging;

namespace EventDrivenWorkflow.Memory.Messaging
{
    public class MessageQueue<TMessage> : IMessageSender<TMessage>
    {
        private readonly object lockObject;
        private readonly List<MessageProcessor<TMessage>> processors;

        public MessageQueue()
        {
            this.lockObject = new object();
            this.processors = new List<MessageProcessor<TMessage>>();
        }

        public void AddProcessor(MessageProcessor<TMessage> processor)
        {
            lock (this.lockObject)
            {
                this.processors.Add(processor);
            }
        }

        public Task Send(TMessage message)
        {
            return this.Send(message, delayDuration: TimeSpan.Zero);
        }

        public Task Send(TMessage message, TimeSpan delayDuration)
        {
            var attempt = new Attempt<TMessage>
            {
                Value = message,
                AttamptCount = 0
            };

            this.Enqueue(attempt, delayDuration);
            return Task.CompletedTask;
        }

        internal void Enqueue(Attempt<TMessage> attempt)
        {
            this.Enqueue(attempt, delayDuration: TimeSpan.Zero);
        }

        internal void Enqueue(Attempt<TMessage> attempt, TimeSpan delayDuration)
        {
            List<MessageProcessor<TMessage>> copy = null;
            lock (this.lockObject)
            {
                copy = processors.ToList();
            }

            foreach (var processor in copy)
            {
                if (delayDuration == TimeSpan.Zero)
                {
                    Task.Run(async () =>
                    {
                        await Task.Yield();
                        await processor.Process(attempt);
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(delayDuration);
                        await processor.Process(attempt);
                    }); ;
                }
            }
        }
    }
}
