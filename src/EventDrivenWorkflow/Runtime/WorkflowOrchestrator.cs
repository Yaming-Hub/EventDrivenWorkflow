// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowOrchestrator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime.MessageHandlers;
    using EventDrivenWorkflow.Runtime.Data;
    using EventDrivenWorkflow.Persistence;

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
            this.InputEventLoader = new InputEventLoader(this);
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

        internal InputEventLoader InputEventLoader { get; }

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
        public Task<WorkflowExecutionContext> StartNew(string partitionKey = null, WorkflowExecutionOptions options = null)
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
        public Task<WorkflowExecutionContext> StartNew<T>(T payload, string partitionKey = null, WorkflowExecutionOptions options = null)
        {
            return StartNew(payloadType: typeof(T), payload: payload, partitionKey: partitionKey, options: options);
        }

        /// <summary>
        /// Start a new workflow with payload.
        /// </summary>
        /// <typeparam name="T">Type of the start event payload.</typeparam>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        public Task<WorkflowExecutionContext> StartNew(QualifiedExecutionContext parentExecutionContext, IReadOnlyDictionary<string, string> eventMap)
        {
            return StartNew(payloadType: null, payload: null, parentExecutionContext: parentExecutionContext, eventMap: eventMap);
        }

        /// <summary>
        /// Start a new workflow with payload.
        /// </summary>
        /// <typeparam name="T">Type of the start event payload.</typeparam>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        public Task<WorkflowExecutionContext> StartNew<T>(T payload, QualifiedExecutionContext parentExecutionContext, IReadOnlyDictionary<string, string> eventMap)
        {
            return StartNew(payloadType: typeof(T), payload: payload, parentExecutionContext: parentExecutionContext, eventMap: eventMap);
        }

        /// <summary>
        /// Start new workflow.
        /// </summary>
        /// <param name="payloadType">Type of the start event payload.</param>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        private Task<WorkflowExecutionContext> StartNew(Type payloadType, object payload, string partitionKey, WorkflowExecutionOptions options)
        {
            Guid executionId = Guid.NewGuid();
            options = options ?? WorkflowExecutionOptions.Default;

            var workflowExecutionContext = new WorkflowExecutionContext
            {
                PartitionKey = partitionKey,
                ExecutionId = executionId,
                WorkflowId = Guid.NewGuid(),
                WorkflowName = this.WorkflowDefinition.Name,
                WorkflowVersion = this.WorkflowDefinition.Version,
                WorkflowStartDateTime = this.Engine.TimeProvider.UtcNow,
                WorkflowExpireDateTime = this.Engine.TimeProvider.UtcNow + this.WorkflowDefinition.MaxExecuteDuration,
                Options = options,
            };

            return StartNew(payloadType, payload, workflowExecutionContext);
        }

        /// <summary>
        /// Start new workflow with parent execution context.
        /// </summary>
        /// <param name="payloadType">Type of the start event payload.</param>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        private Task<WorkflowExecutionContext> StartNew(
            Type payloadType,
            object payload,
            QualifiedExecutionContext parentExecutionContext, 
            IReadOnlyDictionary<string, string> eventMap)
        {
            var workflowExecutionContext = new WorkflowExecutionContext
            {
                ExecutionId = parentExecutionContext.WorkflowExecutionContext.ExecutionId,
                WorkflowName = this.WorkflowDefinition.Name,
                WorkflowVersion = this.WorkflowDefinition.Version,
                PartitionKey = parentExecutionContext.WorkflowExecutionContext.PartitionKey,
                WorkflowStartDateTime = this.Engine.TimeProvider.UtcNow,
                WorkflowExpireDateTime = this.Engine.TimeProvider.UtcNow + this.WorkflowDefinition.MaxExecuteDuration,
                WorkflowId = Guid.NewGuid(),
                Options = parentExecutionContext.WorkflowExecutionContext.Options,
                CallbackInfo = new WorkflowCallbackInfo
                {
                    ActivityExecutionId = parentExecutionContext.ActivityExecutionId,
                    EventMap = new Dictionary<string, string>(eventMap),
                }
            };

            return StartNew(payloadType, payload, workflowExecutionContext);
        }

        /// <summary>
        /// Start new workflow.
        /// </summary>
        /// <param name="payloadType">Type of the start event payload.</param>
        /// <param name="payload">The payload of the start event.</param>
        /// <param name="partitionKey">The partition key of the workflow.</param>
        /// <param name="options">The workflow execution options.</param>
        /// <returns>The workflow id.</returns>
        private async Task<WorkflowExecutionContext> StartNew(
            Type payloadType,
            object payload,
            WorkflowExecutionContext workflowExecutionContext)
        {
            var triggerEventDefinition = this.WorkflowDefinition.TriggerEvent;
            if (triggerEventDefinition.PayloadType != payloadType)
            {
                throw new InvalidOperationException(
                    $"The payload type {payloadType} doesn't match start event payload type {triggerEventDefinition.PayloadType}");
            }

            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Name = triggerEventDefinition.Name,
                DelayDuration = TimeSpan.Zero,
                Value = payload,
                SourceEngineId = this.Engine.Id,
            };

            var startEventMessage = new EventMessage
            {
                EventModel = new EventModel
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    SourceEngineId = @event.SourceEngineId,
                    DelayDuration = @event.DelayDuration,
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

            await this.Engine.Observer.EventPublished(workflowExecutionContext, activityExecutionContext: null, @event);

            await this.Engine.Observer.WorkflowStarted(workflowExecutionContext);

            //if (options.TrackProgress)
            //{
            //    var trackWorkflowTimeoutMessage = new ControlMessage
            //    {
            //        Operation = ControlOperation.WorkflowTimeout,
            //        WorkflowName = workflowExecutionContext.WorkflowName
            //    };

            //    await this.Engine.ControlMessageSender.Send(trackWorkflowTimeoutMessage);
            //}

            return workflowExecutionContext;
        }

        /// <summary>
        /// Finish executing the asynchronous activity.
        /// </summary>
        /// <param name="context">The activity execution context.</param>
        /// <param name="outputEvents">An array contains output events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task EndExecute(QualifiedActivityExecutionId executionId, Action<QualifiedExecutionContext, IEventPublisher> publishOutputEvent)
        {
            var workflowDefinition = this.WorkflowDefinition;

            QualifiedExecutionContext context = null;
            string key = executionId.ToString();

            try
            {
                var contextEntity = await this.Engine.ActivityExecutionContextStore.Get(executionId.PartitionKey, key);
                context = contextEntity.Value;
            }
            catch (StoreException se) when (se.ErrorCode == StoreErrorCode.NotFound)
            {
                throw new WorkflowRuntimeException(
                    isTransient: false,
                    message: $"Cannot complete {executionId} because the context is not found. The activity may have already completed or expired.",
                    se);
            }

            if (workflowDefinition.Name != context.WorkflowExecutionContext.WorkflowName)
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity in workflow {context.WorkflowExecutionContext.WorkflowName} " +
                    $"using completer of workflow {workflowDefinition.Name}.");
            }

            // TODO: Compare workflow version.

            if (!workflowDefinition.ActivityDefinitions.TryGetValue(context.ActivityExecutionContext.ActivityName, out var activityDefinition))
            {
                throw new InvalidOperationException(
                    $"Cannot complete activity {context.ActivityExecutionContext.ActivityName} because it is not " +
                    $"defined in workflow {workflowDefinition.Name}.");
            }

            EventOperator eventOperator = new EventOperator(
                this,
                activityDefinition,
                context,
                inputEvents: new Dictionary<string, Event>());

            if (publishOutputEvent != null)
            {
                publishOutputEvent(context, eventOperator);
            }

            await this.Engine.Observer.ActivityCompleted(context, eventOperator.GetOutputEvents());

            await this.ActivityExecutor.PublishOutputEvents(context, activityDefinition, eventOperator);

            // Delete the context. Please note, if the EndExecute() is called multiple times before the context is deleted
            // it's possible that the output events will be published multiple times. This is acceptable as the workflow 
            // guarantee at least once execution, and downstream activity could be triggered more than once.
            await this.Engine.ActivityExecutionContextStore.Delete(executionId.PartitionKey, key);
        }
    }
}