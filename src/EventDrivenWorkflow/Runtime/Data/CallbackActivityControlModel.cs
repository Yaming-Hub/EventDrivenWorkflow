using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime.Data
{
    public class CallbackActivityControlModel : ControlModel
    {
        public WorkflowCallbackInfo CallbackInfo { get; init; }

        public List<EventModel> Events { get; init; }
    }
}
