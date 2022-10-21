// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowOrchestrator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    using Microsoft.EventDrivenWorkflow.Definitions;
    using Microsoft.EventDrivenWorkflow.Runtime.MessageHandlers;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

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
        /// <param name="executableFactory">The executable factory.</param>
        public WorkflowOrchestrator(
            WorkflowEngine engine,
            WorkflowDefinition workflowDefinition,
            IExecutableFactory executableFactory)
        {
            this.Engine = engine;
            this.WorkflowDefinition = workflowDefinition;
            this.ExecutableFactory = executableFactory;

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
        /// Gets the executable factory.
        /// </summary>
        internal IExecutableFactory ExecutableFactory { get; }

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
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        public Task<Guid> StartNew(string partitionKey = null, WorkflowExecutionOptions options = null)
        {

            return StartNew(payloadType: null, payload: null, partitionKey: partitionKey, options: options);
        }

        /// <summary>
        /// Start a new workflow with payload.
        /// </summary>
        /// <typeparam name="T">Type of the start event payload.</typeparam>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        public Task<Guid> StartNew<T>(T payload, string partitionKey = null, WorkflowExecutionOptions options = null)
        {
            return StartNew(payloadType: typeof(T), payload: payload, partitionKey: partitionKey, options: options);
        }

        /// <summary>
        /// Start new workflow.
        /// </summary>
        /// <param name="payloadType">Type of the start event payload.</param>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        private async Task<Guid> StartNew(Type payloadType, object payload, string partitionKey, WorkflowExecutionOptions options)
        {
            Guid workflowId = Guid.NewGuid();
            options = options ?? WorkflowExecutionOptions.Default;

            var workflowExecutionContext = new WorkflowExecutionContext
            {
                WorkflowName = this.WorkflowDefinition.Name,
                WorkflowVersion = this.WorkflowDefinition.Version,
                PartitionKey = partitionKey,
                WorkflowStartDateTime = this.Engine.TimeProvider.UtcNow,
                WorkflowExpireDateTime = this.Engine.TimeProvider.UtcNow + this.WorkflowDefinition.MaxExecuteDuration,
                WorkflowId = workflowId,
                Options = options,
            };

            var startActivityDefinition = this.WorkflowDefinition.StartActivityDefinition;

            if (startActivityDefinition.InputEventDefinitions.Count == 0)
            {
                if (payloadType != null || payload != null)
                {
                    throw new InvalidOperationException("The workflow cannot start with payload.");
                }

                var executeStartActivityMessage = new Message<ControlModel>
                {
                    Value = new ControlModel
                    {
                        Operation = ControlOperation.ExecuteActivity,
                        TargetActivityName = startActivityDefinition.Name,
                    },
                    WorkflowExecutionContext = workflowExecutionContext,
                };

                // Queue a control message to trigger the start activity.
                await this.Engine.ControlMessageSender.Send(executeStartActivityMessage);
            }
            else
            {
                var startEventDefinition = startActivityDefinition.InputEventDefinitions.Values.First();
                if (startEventDefinition.PayloadType != payloadType)
                {
                    throw new InvalidOperationException(
                        $"The payload type {payloadType} doesn't match start event payload type {startEventDefinition.PayloadType}");
                }

                var startEventMessage = new Message<EventModel>
                {
                    Value = new EventModel
                    {
                        Id = Guid.NewGuid(),
                        Name = startEventDefinition.Name,
                        SourceEngineId = this.Engine.Id,
                        DelayDuration = TimeSpan.Zero,
                        Payload = payloadType == null ? null : new Payload
                        {
                            TypeName = payloadType.FullName,
                            Body = payload == null ? null : this.Engine.Serializer.Serialize(payload),
                        }
                    },
                    WorkflowExecutionContext = workflowExecutionContext,
                };

                // Queue the start event message to trigger the start activity.
                await this.Engine.EventMessageSender.Send(startEventMessage);
            }

            await this.Engine.Observer.WorkflowStarted(workflowExecutionContext);

            if (options.TrackProgress)
            {
                var trackWorkflowTimeoutMessage = new Message<ControlModel>
                {
                    Value = new ControlModel
                    {
                        Operation = ControlOperation.WorkflowTimeout,
                    },
                    WorkflowExecutionContext = workflowExecutionContext,
                };

                await this.Engine.ControlMessageSender.Send(trackWorkflowTimeoutMessage);
            }

            return workflowId;
        }

        /// <summary>
        /// Finish executing the asynchronous activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="outputEvents">An array contains output events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task EndExecute(QualifiedExecutionId qualifiedExecutionId, Action<ActivityExecutionContext, IEventPublisher> publishOutputEvent)
        {
            var workflowDefinition = this.WorkflowDefinition;

            var contextEntity = await this.Engine.ActivityExecutionContextStore.Get(qualifiedExecutionId.PartitionKey, key: qualifiedExecutionId.ToString());
            if (workflowDefinition.Name != contextEntity.Value.WorkflowName)
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity in workflow {contextEntity.Value.WorkflowName} " +
                    $"using completer of workflow {workflowDefinition.Name}.");
            }

            // TODO: Compare workflow version.

            if (!workflowDefinition.ActivityDefinitions.TryGetValue(contextEntity.Value.ActivityName, out var activityDefinition))
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity {contextEntity.Value.ActivityName} because it is not " +
                    $"defined in workflow {workflowDefinition.Name}.");
            }

            EventOperator eventOperator = new EventOperator(
                this,
                activityDefinition,
                contextEntity.Value,
                inputEvents: new Dictionary<string, Event>());

            if (publishOutputEvent != null)
            {
                publishOutputEvent(contextEntity.Value, eventOperator);
            }

            await this.Engine.Observer.ActivityCompleted(contextEntity.Value, eventOperator.GetOutputEvents());

            await this.ActivityExecutor.PublishOutputEvents(contextEntity.Value, activityDefinition, eventOperator);
        }
    }
}