using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    /// <summary>
    /// This class defines the message use to control workflow execution.
    /// </summary>
    public class ControlMessage : MessageBase
    {
        public ControlOperation Operation { get; init; }

        public ErrorInfo Error { get; init; }

        public string TargetActivityName { get; init; }
    }
}
