using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenWorkflow.Runtime
{
    public interface IWorkflowOrchestratorProvider
    {
        WorkflowOrchestrator GetWorkflowOrchestrator(string workflowName);
    }
}
