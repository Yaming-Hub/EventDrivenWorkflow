using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Contract.Persistence;
using Microsoft.EventDrivenWorkflow.Core.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Persistence;

namespace Microsoft.EventDrivenWorkflow.Core
{
    public sealed class WorkflowEngine
    {
        public WorkflowEngine(
            IMessageProcessor<EventMessage> eventMessageProcessor,
            IMessageProcessor<ControlMessage> controlMessageProcessor,
            IMessageSender<EventMessage> eventMessageSender,
            IMessageSender<ControlMessage> controlMessageSender,
            ISerializer serializer,
            IStore<EventEntity> eventStore,
            IStore<ActivityEntity> activityStore)
        {
            this.EventMessageProcessor = eventMessageProcessor;
            this.ControlMessageProcessor = controlMessageProcessor;
            this.EventMessageSender = eventMessageSender;
            this.ControlMessageSender = controlMessageSender;
            this.Serializer = serializer;
            this.EventStore = eventStore;
            this.ActivityStore = activityStore;
        }

        public IMessageProcessor<EventMessage> EventMessageProcessor { get; }

        public IMessageProcessor<ControlMessage> ControlMessageProcessor { get; }

        public IMessageSender<EventMessage> EventMessageSender { get; }

        public IMessageSender<ControlMessage> ControlMessageSender { get; }

        public ISerializer Serializer { get; }

        public IStore<EventEntity> EventStore { get; }

        public IStore<ActivityEntity> ActivityStore { get; }
    }
}
