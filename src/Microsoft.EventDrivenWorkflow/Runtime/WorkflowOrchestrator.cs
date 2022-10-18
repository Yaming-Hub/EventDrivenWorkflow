// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowOrchestrator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers;
    using Microsoft.EventDrivenWorkflow.Runtime.Model;

    /// <summary>
    /// This class orchestrates the workflow execution. It can start a new workflow and
    /// complete asynchronous activity execution to resume the workflow.
    /// </summary>
    public sealed class WorkflowOrchestrator : IDisposable
    {
        /// <summary>
        /// The message handler handles event message of the workflow.
        /// </summary>
        private readonly EventMessageHandler eventMessageHandler;

        /// <summary>
        /// The message handler handles control message of the workflow.
        /// </summary>
        private readonly ControlMessageHandler controlMessageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowOrchestrator"/> class.
        /// </summary>
        /// <param name="engine">The workflow engine.</param>
        /// <param name="workflowDefinition">The workflow definition.</param>
        /// <param name="activityFactory">The activity factory.</param>
        /// <param name="options">The workflow orchestration options.</param>
        public WorkflowOrchestrator(
            WorkflowEngine engine,
            WorkflowDefinition workflowDefinition,
            IActivityFactory activityFactory,
            WorkflowOrchestrationOptions options)
        {
            this.Engine = engine;
            this.WorkflowDefinition = workflowDefinition;
            this.ActivityFactory = activityFactory;
            this.Options = options;

            this.eventMessageHandler = new EventMessageHandler(this);
            this.controlMessageHandler = new ControlMessageHandler(this);
            this.ActivityExecutor = new ActivityExecutor(this);

            this.Engine.EventMessageProcessor.Subscribe(this.eventMessageHandler);
            this.Engine.ControlMessageProcessor.Subscribe(this.controlMessageHandler);
        }

        /// <summary>
        /// Gets the workflow engine.
        /// </summary>
        internal WorkflowEngine Engine { get; }

        /// <summary>
        /// Gets the workflow definition.
        /// </summary>
        internal WorkflowDefinition WorkflowDefinition { get; }

        /// <summary>
        /// Gets the activity factory.
        /// </summary>
        internal IActivityFactory ActivityFactory { get; }

        /// <summary>
        /// Gets the workflow orchestration options.
        /// </summary>
        internal WorkflowOrchestrationOptions Options { get; }

        /// <summary>
        /// Gets the activity executor.
        /// </summary>
        internal ActivityExecutor ActivityExecutor { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Engine.ControlMessageProcessor.Unsubscribe(this.controlMessageHandler);
            this.Engine.EventMessageProcessor.Unsubscribe(this.eventMessageHandler);
        }

        /// <summary>
        /// Start a new workflow.
        /// </summary>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <returns>The workflow id.</returns>
        public async Task<Guid> StartNew(string partitionKey = null)
        {
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

            if (this.Options.TrackProgress)
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

        /// <summary>
        /// Finish executing the asynchronous activity.
        /// </summary>
        /// <param name="activityExecutionInfo"></param>
        /// <param name="outputEvents"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task EndExecute(ActivityExecutionInfo activityExecutionInfo, params Event[] outputEvents)
        {
            var workflowDefinition = this.WorkflowDefinition;
            if (workflowDefinition.Name != activityExecutionInfo.WorkflowName)
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity in workflow {activityExecutionInfo.WorkflowName} " +
                    $"using completer of workflow {workflowDefinition.Name}.");
            }

            // TODO: Compare workflow version.

            if (!workflowDefinition.ActivityDefinitions.TryGetValue(activityExecutionInfo.ActivityName, out var activityDefinition))
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity {activityExecutionInfo.ActivityName} because it is not " +
                    $"defined in workflow {workflowDefinition.Name}.");
            }

            ActivityExecutionContext activityExecutionContext = new ActivityExecutionContext(
                workflowDefinition,
                activityDefinition,
                activityExecutionInfo,
                inputEvents: new Dictionary<string, EventData>());

            activityExecutionContext.PublishEventInternal(outputEvents);

            await this.ActivityExecutor.PublishOutputEvents(activityExecutionContext);
        }
    }
}