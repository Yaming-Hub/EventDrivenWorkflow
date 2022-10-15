using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;
using Microsoft.EventDrivenWorkflow.Contract.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Messaging;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class WorkflowEventMessageHandler : IMessageHandler<EventMessage>
    {
        private readonly WorkflowOrchestrator ochestrator;

        public WorkflowEventMessageHandler(WorkflowOrchestrator workflowEngine)
        {
            this.ochestrator = workflowEngine;
        }

        public async Task<MessageHandleResult> Handle(EventMessage item)
        {
            var workflowDefinition = this.ochestrator.WorkflowDefinition;
            if (item.WorkflowExecutionInfo.WorkflowName != workflowDefinition.Name)
            {
                return MessageHandleResult.Ignore; // Ignore if the event doesn't belong to this workflow.
            }

            var eventDefinition = workflowDefinition.EventDefinitions.FirstOrDefault(ed => ed.Name == item.EventName);
            if (eventDefinition == null)
            {
                // TODO: Got an unknown event. The workflow definition may changed. Report and ignore.
                return MessageHandleResult.Ignore;
            }

            if (eventDefinition.PayloadType.FullName != item.PayloadType)
            {
                // TODO: The incoming event payload type doesn't match the event definition. Report and ignore.
                return MessageHandleResult.Ignore;
            }

            // Find the activity that subscribe to the current event. Please note, there should be exactly
            // one activity listen to the event.
            var activityDefinition = workflowDefinition.ActivityDefinitions.FirstOrDefault(a => a.InputEventDefinitions.Any(e => e.Name == item.EventName));
            if (activityDefinition == null)
            {
                // This should not happen, workflow definition will be invalid if there is any orphan event.
                return MessageHandleResult.Completed;
            }

            // var = this.workflowEngine.Serializer.Deserialize(item.Payload, item.PayloadType)
            var coreEvent = GetCoreEvent(item);
            var partitionKey = item.WorkflowExecutionInfo.PartitionKey;
            var info = item.WorkflowExecutionInfo;

            if (activityDefinition.InputEventDefinitions.Any(e => e.Name == item.EventName))
            {
                // Check if we got all incoming events
                var inputEvents = new Dictionary<string, Event>(capacity: activityDefinition.InputEventDefinitions.Count);
                inputEvents[item.EventName] = coreEvent;

                if (activityDefinition.InputEventDefinitions.Count > 1)
                {
                    // TODO: Handle multiple execution state.
                    // To support multiple input events

                    // var result = this.workflowEngine.EventStore.Get()
                    // The event key is in "{WorkflowName}/{WorkflowId}/events/{EventName}[{PartitionKey}]" format.
                    var keys = activityDefinition.InputEventDefinitions
                        .Where(e => e.Name != item.EventName)
                        .Select(e => GetEventKey(workflowDefinition.Name, info.WorkflowId, e.Name, info.PartitionKey));

                    var persistedEvents = await this.ochestrator.Engine.EventStore.GetMany(partitionKey, keys);
                    if (persistedEvents.Count == activityDefinition.InputEventDefinitions.Count - 1)
                    {
                        foreach (var pe in persistedEvents)
                        {
                            // TODOinputEvents[pe.Key] = GetCoreEvent(pe.Value);
                        }
                    }
                    else
                    {
                        // Not all incoming events are ready at this point. Put current event into store and move on
                        // TODO: Queue a timeout control
                        return MessageHandleResult.Completed;
                    }
                }

                // Execute events when all inputs are ready.
                await this.ochestrator.ActivityExecutor.Execute(
                    workflowDefinition,
                    activityDefinition,
                    info,
                    inputEvents);
            }

            return MessageHandleResult.Completed;
        }

        private Event GetCoreEvent(EventMessage eventMessage)
        {
            throw new NotImplementedException();
        }

        private static string GetEventKey(string workflowName, Guid workflowId, string eventName, string partitionKey)
        {
            return $"{workflowName}/{workflowId}/events/{eventName}[{partitionKey}]";
        }
    }
}
