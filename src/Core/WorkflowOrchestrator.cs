using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Core.MessageHandlers;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
{
    /// <summary>
    /// This class orchestrates the workflow execution.
    /// </summary>
    public sealed class WorkflowOrchestrator : IDisposable
    {
        private readonly EventMessageHandler eventMessageHandler;

        private readonly ExecuteActivityMessageHandler executeActivityMessageHandler;

        public WorkflowOrchestrator(
            WorkflowEngine engine,
            WorkflowDefinition workflowDefinition,
            IActivityFactory activityFactory,
            WorkflowOptions options)
        {
            this.Engine = engine;
            this.WorkflowDefinition = workflowDefinition;
            this.ActivityFactory = activityFactory;
            this.Options = options;

            this.eventMessageHandler = new EventMessageHandler(this);
            this.executeActivityMessageHandler = new ExecuteActivityMessageHandler(this);
            this.AsyncActivityCompleter = new AsyncActivityCompleter(this);
            this.ActivityExecutor = new ActivityExecutor(this);

            this.Engine.EventMessageProcessor.Subscribe(this.eventMessageHandler);
            this.Engine.ControlMessageProcessor.Subscribe(this.executeActivityMessageHandler);
        }

        public WorkflowEngine Engine { get; }

        public WorkflowDefinition WorkflowDefinition { get; }

        public IActivityFactory ActivityFactory { get; }

        public IAsyncActivityCompleter AsyncActivityCompleter { get; }

        public WorkflowOptions Options { get; }

        internal ActivityExecutor ActivityExecutor { get; }

        public void Dispose()
        {
            this.Engine.ControlMessageProcessor.Unsubscribe(this.executeActivityMessageHandler);
            this.Engine.EventMessageProcessor.Unsubscribe(this.eventMessageHandler);
        }

        public async Task<Guid> StartNew(string partitionKey)
        {
            if (this.WorkflowDefinition.Type == WorkflowType.Open)
            {
                throw new InvalidOperationException("A open workflow cannot start by itself.");
            }

            Guid workflowId = Guid.NewGuid();

            var workflowExecutionInfo = new WorkflowExecutionInfo
            {
                WorkflowName = this.WorkflowDefinition.Name,
                WorkflowVersion = this.WorkflowDefinition.Version,
                PartitionKey = partitionKey ?? string.Empty,
                WorkflowStartDateTime = DateTime.UtcNow,
                WorkflowId = workflowId,
            };

            var executeInitializingActivityMessage = new ControlMessage
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionInfo = workflowExecutionInfo,
                Operation = ControlOperation.ExecuteActivity,
                TargetActivityName = this.WorkflowDefinition.InitializingActivityDefinition.Name,
            };

            // Queue a control message to start the initialing activity.
            await this.Engine.ControlMessageSender.Send(executeInitializingActivityMessage);

            if (this.Options.TrackWorkflow)
            {
                var trackWorkflowTimeoutMessage = new ControlMessage
                {
                    Id = Guid.NewGuid(),
                    WorkflowExecutionInfo = workflowExecutionInfo,
                    Operation = ControlOperation.WorkflowTimeout,
                };

                await this.Engine.ControlMessageSender.Send(trackWorkflowTimeoutMessage);
            }

            return workflowId;
        }
    }
}