using Microsoft.EventDrivenWorkflow.Messaging;
using Microsoft.EventDrivenWorkflow.Persistence;
using Microsoft.EventDrivenWorkflow.Core.Model;

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
            IEntityStore<EventEntity> eventStore,
            IEntityStore<ActivityStateEntity> activityStore)
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

        public IEntityStore<EventEntity> EventStore { get; }

        public IEntityStore<ActivityStateEntity> ActivityStore { get; }
    }
}
