using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Core.Model;

namespace Microsoft.EventDrivenWorkflow.Core.Messaging
{
    /// <summary>
    /// This class defines the message use to control workflow execution.
    /// </summary>
    public class ControlMessage : MessageBase
    {
        public ControlMessageType ControlType { get; init; }

        public ErrorInfo Error { get; init; }

        public string TargetActivityName { get; init; }
    }
}
