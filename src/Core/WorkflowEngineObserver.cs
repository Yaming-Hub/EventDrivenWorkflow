using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;
using Microsoft.EventDrivenWorkflow.Contract.Provider;
using Microsoft.EventDrivenWorkflow.Core.Contract;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class WorkflowEngineObserver : IAsyncObserver<EventMessage>
    {
        private readonly WorkflowEngine workflowEngine;
        private readonly WorkflowExecutionContext executionContext;

        public WorkflowEngineObserver(WorkflowEngine workflowEngine)
        {
            this.workflowEngine = workflowEngine;
            this.executionContext = executionContext;
        }

        public Task OnComplete()
        {
            throw new NotImplementedException();
        }

        public Task OnError(Exception exception)
        {
            throw new NotImplementedException();
        }

        public async Task OnNext(EventMessage item)
        {
            var workflowDefinition = this.workflowEngine.WorkflowDefinition;
            if (item.WorkflowName != workflowDefinition.Name)
            {
                return; // Ignore if the event doesn't belong to this workflow.
            }

            var eventDefinition = workflowDefinition.EventDefinitions.FirstOrDefault(ed => ed.Name == item.EventName);
            if (eventDefinition == null)
            {
                // TODO: Got an unknown event. The workflow definition may changed. Report and ignore.
                return;
            }

            if (eventDefinition.PayloadType.FullName != item.PayloadType)
            {
                // TODO: The incoming event payload type doesn't match the event definition. Report and ignore.
                return;
            }


            // var = this.workflowEngine.Serializer.Deserialize(item.Payload, item.PayloadType)
            var coreEvent = GetCoreEvent(item); 

            foreach (var activity in workflowDefinition.ActivityDefinitions)
            {
                if (activity.EventsToSubscribe.Any(e => e.Name == item.EventName))
                {
                    // Check if we got all incoming events
                    Dictionary<string, object> eventPayloads = new Dictionary<string, object>(capacity: activity.EventsToSubscribe.Count);
                    eventPayloads[item.EventName] = coreEvent;

                    if (activity.EventsToSubscribe.Count > 1)
                    {
                        // var result = this.workflowEngine.EventStore.Get()
                        var keys = activity.EventsToSubscribe
                            .Where(e => e.Name != item.EventName)
                            .Select(e => new EventKey
                            {
                                WorkflowName = workflowDefinition.Name,
                                EventName = e.Name,
                                Partition = item.Partition,
                                WorkflowExecutionId = item.WorkflowExecutionId
                            });

                        var persistedEvents = await this.workflowEngine.EventStore.Get(keys);
                        if (persistedEvents.Count == activity.EventsToSubscribe.Count - 1)
                        {
                            foreach (var pe in persistedEvents)
                            {
                                eventPayloads[pe.Key.EventName] = GetCoreEvent(pe.Value);
                            }
                        }
                        else
                        {
                            // Not all incoming events are ready at this point. Put current event into store and move on
                            // TODO: Queue a timeout control
                        }
                    }
                }

            }


        }

        private CoreEvent GetCoreEvent(EventMessage eventMessage)
        {
            throw new NotImplementedException();
        }
    }
}
