using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.Messaging
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