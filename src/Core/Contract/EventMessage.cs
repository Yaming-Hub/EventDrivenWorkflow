using Microsoft.EventDrivenWorkflow.Core.Contract;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class EventMessage
    {
        public string WorkflowName { get; set; }

        public string EventName { get; set; }

        public string Partition { get; }

        public Guid WorkflowExecutionId { get; set; }

        public DateTime CreateDateTime { get; set; }

        public string SourceActivityName { get; set; }

        public Guid SourceActivityExecutionId { get; set; }

        public byte[] Payload { get; set; }

        public string PayloadType { get; set; }

        public EventKey GetKey => new EventKey
        {
            WorkflowName = WorkflowName,
            Partition = Partition,
            WorkflowExecutionId = WorkflowExecutionId,
            EventName = EventName,
        };
    }
}