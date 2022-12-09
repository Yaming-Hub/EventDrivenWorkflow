// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityExecutor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime
{
    using System.Text;
    using EventDrivenWorkflow.Definitions;
    using EventDrivenWorkflow.Runtime.Data;
    using EventDrivenWorkflow.Utilities;

    /// <summary>
    /// This class defines the activity executor.
    /// </summary>
    internal sealed class ActivityExecutor
    {
        /// <summary>
        /// The workflow orchestrator.
        /// </summary>
        private readonly WorkflowOrchestrator orchestrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityExecutor"/> class.
        /// </summary>
        /// <param name="orchestrator">The workflow orchestrator.</param>
        public ActivityExecutor(WorkflowOrchestrator orchestrator)
        {
            this.orchestrator = orchestrator;
        }

        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="workflowExecutionContext">The workflow execution context.</param>
        /// <param name="activityDefinition">The activity definition.</param>
        /// <param name="eventModel">The triggering event model.</param>
        /// <param name="inputEvents">A dictionary contains input events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task<ExecutionContext> Execute(
            WorkflowExecutionContext workflowExecutionContext,
            ActivityDefinition activityDefinition,
            IReadOnlyDictionary<string, Event> inputEvents,
            EventModel triggerEvent)
        {
            // Construct activity execution context.
            var context = new ExecutionContext
            {
                WorkflowExecutionContext = workflowExecutionContext,
                ActivityExecutionContext = new ActivityExecutionContext
                {
                    ActivityName = activityDefinition.Name,
                    ActivityExecutionStartDateTime = this.orchestrator.Engine.TimeProvider.UtcNow,
                    ActivityId = CaculateActivityId(activityDefinition.Name, inputEvents),
                    AttemptCount = 0,
                    TriggerEventReference = triggerEvent == null ? null : new EventReference
                    {
                        Id = triggerEvent.Id,
                        Name = triggerEvent.Name,
                    }
                }
            };

            await this.Execute(context, activityDefinition, inputEvents, triggerEvent);
            return context;
        }

        /// <summary>
        /// Execute the activity.
        /// </summary>
        /// <param name="wec">The workflow execution context.</param>
        /// <param name="activityDefinition">The activity definition.</param>
        /// <param name="eventModel">The triggering event model.</param>
        /// <param name="inputEvents">A dictionary contains input events.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task Execute(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            IReadOnlyDictionary<string, Event> inputEvents,
            EventModel triggerEventModel)
        {
            // Now we got all incoming events for the activity, let's run it.
            var eventOperator = new EventOperator(
                this.orchestrator,
                activityDefinition: activityDefinition,
                context: context,
                inputEvents: inputEvents);

            eventOperator.ValidateInputEvents();

            if (activityDefinition.IsCompleteActivity)
            {
                await this.ExecuteComplete(context, inputEvents);
            }
            else if (activityDefinition.IsAsync)
            {
                await this.ExecuteAsync(
                    context,
                    activityDefinition,
                    eventOperator,
                    triggerEventModel);
            }
            else
            {
                await this.ExecuteSync(
                    context,
                    activityDefinition,
                    eventOperator,
                    triggerEventModel);
            }
        }

        /// <summary>
        /// Publish output events.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        public async Task PublishOutputEvents(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator)
        {
            eventOperator.ValidateOutputEvents();

            // Queue the output events.
            var ouptutEvents = eventOperator.GetOutputEvents().ToList();
            foreach (var outputEvent in ouptutEvents)
            {
                var eventDefinition = activityDefinition.OutputEventDefinitions[outputEvent.Name];
                var payloadType = eventDefinition.PayloadType;
                object payload = outputEvent.GetPayload(payloadType);

                var message = new EventMessage
                {
                    EventModel = new EventModel
                    {
                        Id = outputEvent.Id,
                        SourceEngineId = outputEvent.SourceEngineId,
                        Name = outputEvent.Name,
                        Payload = new Payload
                        {
                            TypeName = payloadType?.FullName,
                            Body = payload == null ? null : this.orchestrator.Engine.Serializer.Serialize(payload),
                        },
                        SourceActivity = new ActivityReference
                        {
                            Name = context.ActivityExecutionContext.ActivityName,
                            ExecutionId = context.ActivityExecutionContext.ActivityId,
                        }
                    },
                    WorkflowExecutionContext = context.WorkflowExecutionContext, // Trim the activity part from context
                };

                await this.orchestrator.Engine.EventMessageSender.Send(message, outputEvent.DelayDuration);

                await this.orchestrator.Engine.Observer.EventPublished(context, outputEvent);
            }

            // TODO: If the workflow is being tracked and there is no output event published
            //       by the current activity. This is one terminating operation, and we should
            //       check if there is any pending events. If there is no pending events other
            //       than the current one, then we can consider workflow as completed. This 
            //       should also work for async activity.
            if (context.WorkflowExecutionContext.Options.TrackProgress)
            {
                // Please note, for workflow completeness tracking, use the workflow partition key instead.
                // The workflow partition key is in "[{partition}]{workflowName}/{workflowId}" format.
                var partitionKey = ResourceKeyFormat.GetWorkflowPartition(context.WorkflowExecutionContext);
                if (ouptutEvents.Count > 0)
                {
                    foreach (var outputEvent in ouptutEvents)
                    {
                        await this.orchestrator.Engine.EventPresenseStore.Upsert(
                            partitionKey: partitionKey,
                            key: ResourceKeyFormat.GetEventKey(
                                partitionKey: context.WorkflowExecutionContext.PartitionKey,
                                executionId: context.WorkflowExecutionContext.ExecutionId,
                                workflowName: context.WorkflowExecutionContext.WorkflowName,
                                workflowId: context.WorkflowExecutionContext.WorkflowId,
                                eventName: outputEvent.Name,
                                eventId: outputEvent.Id),
                            new Entity<EventReference>
                            {
                                Value = new EventReference
                                {
                                    Id = outputEvent.Id,
                                    Name = outputEvent.Name
                                },
                                ExpireDateTime = context.WorkflowExecutionContext.WorkflowExpireDateTime
                            });
                    }
                }
                else
                {
                    // The activity returns no output event, check if the workflow has completed.
                    // If this is the last activity to complete, then there should be only one event
                    // which is the current trigger event present for the whole workflow.
                    bool workflowHasCompleted = false;
                    var activeEvents = await this.orchestrator.Engine.EventPresenseStore.List(partitionKey);
                    if (activeEvents.Count < 1)
                    {
                        // This could only happen to the start activity without any input event. Otherwise, this
                        // is a invalid state for the workflow.
                        if (context.ActivityExecutionContext.TriggerEventReference == null)
                        {
                            throw new WorkflowRuntimeException(isTransient: false, "At least one active event should be found.");
                        }

            //            workflowHasCompleted = true;
            //        }
            //        else if (activeEvents.Count == 1 && context.ActivityExecutionContext.TriggerEventReference != null)
            //        {
            //            // If there is only one active event and this execution has trigger event, then the active event
            //            // must match trigger event. Otherwise, it's an invalid workflow state.
            //            if (activeEvents[0].Value.Name != context.ActivityExecutionContext.TriggerEventReference.Name ||
            //                activeEvents[0].Value.Id != context.ActivityExecutionContext.TriggerEventReference.Id)
            //            {
            //                throw new WorkflowRuntimeException(isTransient: false, "The active event doesn't match.");
            //            }

            //            workflowHasCompleted = true;
            //        }

                    if (workflowHasCompleted)
                    {
                        await this.orchestrator.Engine.Observer.WorkflowCompleted(context.WorkflowExecutionContext, null);
                    }
                    else
                    {
                        // TODO: It's possible that the previous activity is being completed and the trigger event
                        //       hasn't been deleted yet. In this case, we need to queue another control message to
                        //       check again later.
                    }
                }
            }
        }

        /// <summary>
        /// Execute a sync activity.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteSync(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator,
            EventModel triggerEvent)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateExecutable(context.ActivityExecutionContext.ActivityName);

            await this.orchestrator.Engine.Observer.ActivityStarting(context, eventOperator.GetInputEvents());

            var cancellationTokenSource = new CancellationTokenSource(delay: activityDefinition.MaxExecuteDuration);

            bool succeeded = false;

            try
            {
                await activity.Execute(
                    context,
                    eventRetriever: eventOperator,
                    eventPublisher: eventOperator,
                    cancellationToken: cancellationTokenSource.Token);

                succeeded = true;

                await this.orchestrator.Engine.Observer.ActivityCompleted(context, eventOperator.GetOutputEvents());
            }
            catch (OperationCanceledException oce) when (oce.CancellationToken == cancellationTokenSource.Token)
            {
                await this.orchestrator.Engine.Observer.ActivityExecutionTimeout(context);

                // TODO: Activity timeout
            }
            catch (Exception e)
            {
                await this.orchestrator.Engine.Observer.ActivityExecutionFailed(e, context);

                // TODO: Handle exceptions.
            }
            finally
            {
                if (activity is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
                else if (activity is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
            }

            if (succeeded)
            {
                await this.PublishOutputEvents(context, activityDefinition, eventOperator);
            }
            else
            {
                if (context.ActivityExecutionContext.AttemptCount < activityDefinition.RetryPolicy.MaxRetryCount)
                {
                    var controlMessage = new ControlMessage
                    {
                        ControlModel = new ExecuteActivityControlModel
                        {
                            Event = triggerEvent,
                            Operation = ControlOperation.ExecuteActivity,
                            TargetActivityName = activityDefinition.Name,
                            ExecutionContext = new ExecutionContext
                            {
                                WorkflowExecutionContext = context.WorkflowExecutionContext,
                                ActivityExecutionContext = IncrementAttemptCount(context.ActivityExecutionContext),
                            }                            
                        },
                        
                    };

                    await this.orchestrator.Engine.ControlMessageSender.Send(controlMessage, activityDefinition.RetryPolicy.DelayDuration);
                }
                else
                {
                    // Max retry count limit is reached.
                    // TODO: Abandon activity.
                }
            }
        }

        /// <summary>
        /// Execute an async activity.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteAsync(
            ExecutionContext context,
            ActivityDefinition activityDefinition,
            EventOperator eventOperator,
            EventModel triggerEvent)
        {
            var activity = this.orchestrator.ExecutableFactory.CreateAsyncExecutable(context.ActivityExecutionContext.ActivityName);
            await this.orchestrator.Engine.ActivityExecutionContextStore.Upsert(
                partitionKey: context.WorkflowExecutionContext.PartitionKey,
                key: context.ActivityExecutionId.ToString(),
                new Entity<ExecutionContext>
                {
                    Value = context,
                    ExpireDateTime = context.WorkflowExecutionContext.WorkflowExpireDateTime,
                });

            try
            {
                await this.orchestrator.Engine.Observer.ActivityStarting(context, eventOperator.GetInputEvents());

                await activity.BeginExecute(
                    context: context,
                    eventRetriever: eventOperator);
            }
            catch
            {
                // TODO (ymliu): Handle exceptions.
            }
            finally
            {
                if (activity is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
                else if (activity is IAsyncDisposable asyncDisposable)
                {
                    try
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    catch
                    {
                        // Ignore dispose exceptions
                    }
                }
            }
        }

        /// <summary>
        /// Execute the complete activity.
        /// </summary>
        /// <param name="aec">The activity execution context.</param>
        /// <param name="eventOperator">The event operator.</param>
        /// <returns>A task represents the async operation.</returns>
        private async Task ExecuteComplete(
            ExecutionContext context,
            IReadOnlyDictionary<string, Event> inputEvents)
        {
            await this.orchestrator.Engine.Observer.WorkflowCompleted(context.WorkflowExecutionContext, inputEvents.Select(x => x.Value));

            var callbackInfo = context.WorkflowExecutionContext.CallbackInfo;
            if (callbackInfo != null)
            {
                // Resume parent workflow if callback info is available.
                var parentWorkflowName = callbackInfo.ActivityExecutionId.WorkflowName;
                var parentOrchestrator = this.orchestrator.Engine.WorkflowOrchestratorProvider.GetWorkflowOrchestrator(parentWorkflowName);
                if (parentOrchestrator == null)
                {
                    throw new WorkflowRuntimeException(isTransient: false, $"The orchestartor of workflow \"{parentWorkflowName}\"is not found.");
                }

                await parentOrchestrator.EndExecute(
                    executionId: callbackInfo.ActivityExecutionId,
                    publishOutputEvent: (executionContext, publisher) =>
                    {
                        foreach (var inputEvent in inputEvents)
                        {
                            if (callbackInfo.EventMap.TryGetValue(inputEvent.Key, out string outputEventName))
                            {
                                if (!parentOrchestrator.WorkflowDefinition.EventDefinitions.TryGetValue(outputEventName, out var outputEvent))
                                {
                                    throw new WorkflowRuntimeException(isTransient: false, $"The mapped output event \"{outputEventName}\" is not defined.");
                                }

                                object payload;
                                try
                                {
                                    payload = inputEvent.Value.GetPayload(outputEvent.PayloadType);
                                }
                                catch (InvalidCastException e)
                                {
                                    throw new WorkflowRuntimeException(
                                        isTransient: false,
                                        $"The input event \"{inputEvent.Key}\" payload type doesn't match mapped output event \"{outputEventName}\".");
                                }

                                publisher.PublishEvent(outputEventName, payload);
                            }
                        }
                    });
            }
        }


        /// <summary>
        /// Copy workflow execution context.
        /// </summary>
        /// <param name="workflowExecutionContext">The source workflow execution context.</param>
        /// <returns>The copied workflow execution context.</returns>
        private static WorkflowExecutionContext CopyWorkflowExecutionInfo(WorkflowExecutionContext workflowExecutionContext)
        {
            return new WorkflowExecutionContext
            {
                PartitionKey = workflowExecutionContext.PartitionKey,
                WorkflowName = workflowExecutionContext.WorkflowName,
                WorkflowVersion = workflowExecutionContext.WorkflowVersion,
                WorkflowId = workflowExecutionContext.WorkflowId,
                WorkflowStartDateTime = workflowExecutionContext.WorkflowStartDateTime,
                WorkflowExpireDateTime = workflowExecutionContext.WorkflowExpireDateTime,
                Options = workflowExecutionContext.Options,
            };
        }

        private static ActivityExecutionContext IncrementAttemptCount(ActivityExecutionContext activityExecutionContext)
        {
            return new ActivityExecutionContext
            {
                ActivityExecutionStartDateTime = activityExecutionContext.ActivityExecutionStartDateTime,
                ActivityName = activityExecutionContext.ActivityName,
                ActivityId = activityExecutionContext.ActivityId,

                AttemptCount = activityExecutionContext.AttemptCount + 1,
                TriggerEventReference = activityExecutionContext.TriggerEventReference,
            };
        }

        /// <summary>
        /// Calculate activity execution id based on input events.
        /// </summary>
        /// <param name="inputEvents">The input events.</param>
        /// <returns>The calculated activity execution id.</returns>
        private static Guid CaculateActivityId(string activityName, IReadOnlyDictionary<string, Event> inputEvents)
        {
            var sb = new StringBuilder(capacity: activityName.Length + 1 + 37 * inputEvents.Count);
            sb.Append(activityName).Append(":");
            foreach (var id in inputEvents.Values.Select(e => e.Id).OrderBy(x => x))
            {
                sb.Append(id).Append(";");
            }

            return MurmurHash3.HashToGuid(sb.ToString());
        }
    }
}
