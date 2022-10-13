using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Provider;
using Microsoft.EventDrivenWorkflow.Core.Contract;

namespace Microsoft.EventDrivenWorkflow.Core
{
    public class WorkflowEngine
    {
        private readonly IAsyncSender<EventMessage> sender;

        private readonly IAsyncObservable<EventMessage> asyncObservable;

        private readonly WorkflowEngineObserver observer;

        public WorkflowEngine(
            WorkflowDefinition workflowDefinition,
            IActivityFactory activityFactory,
            IAsyncSender<EventMessage> sender,
            ISerializer serializer,
            IAsyncObservable<EventMessage> asyncObservable,
            IStore<EventKey, EventMessage> eventStore)
        {
            this.WorkflowDefinition = workflowDefinition;
            this.ActivityFactory = activityFactory;
            this.sender = sender;
            this.asyncObservable = asyncObservable;
            this.Serializer = serializer;

            this.observer = new WorkflowEngineObserver(this);

            EventStore = eventStore;
        }

        public WorkflowDefinition WorkflowDefinition { get; }

        public IActivityFactory ActivityFactory { get; }

        public ISerializer Serializer { get; }

        public IStore<EventKey, EventMessage> EventStore { get; }

        public Task StartNew()
        {
            // TODO: Start the start activity.

            this.asyncObservable.Subscribe(this.observer);

            return Task.CompletedTask;
        }

    }
}