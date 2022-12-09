// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecuteActivityOperationHandler.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.MessageHandlers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EventDrivenWorkflow.Messaging;
    using EventDrivenWorkflow.Runtime.Data;

    internal sealed class CallbackActivityOperationHandler : IControlOperationHandler
    {
        public async Task<MessageHandleResult> Handle(WorkflowOrchestrator orchestrator, ControlModel controlModel)
        {
            var model = (CallbackActivityControlModel)controlModel;
            var callbackInfo = model.CallbackInfo;

            await orchestrator.EndExecute(
                executionId: callbackInfo.ActivityExecutionId,
                publishOutputEvent: (executionContext, publisher) =>
                {
                    foreach (var inputEvent in model.Events)
                    {
                        if (callbackInfo.EventMap.TryGetValue(inputEvent.Name, out string outputEventName))
                        {
                            if (!orchestrator.WorkflowDefinition.EventDefinitions.TryGetValue(outputEventName, out var outputEventDefinition))
                            {
                                throw new WorkflowRuntimeException(isTransient: false, $"The mapped output event \"{outputEventName}\" is not defined.");
                            }

                            if (inputEvent.Payload != null)
                            {
                                // Make sure the payload type matches output event definition
                                if (inputEvent.Payload.TypeName != outputEventDefinition.PayloadType?.FullName)
                                {
                                    throw new WorkflowRuntimeException(isTransient: false, $"The mapped output event \"{outputEventName}\" is not type mismatch.");
                                }

                                // Directly publish the wrapped payload to avoid deserialization and serialization.
                                publisher.PublishEvent(outputEventName, inputEvent.Payload);
                            }
                            else
                            {
                                publisher.PublishEvent(outputEventName);
                            }
                        }
                    }
                });

            return MessageHandleResult.Complete;
        }
    }
}
