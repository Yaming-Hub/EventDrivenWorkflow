namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class EventMessage : MessageBase
    {
        public string EventName { get; set; }

        public string SourceActivityName { get; set; }

        public Guid SourceActivityExecutionId { get; set; }

        public byte[] Payload { get; set; }

        public string PayloadType { get; set; }
    }
}