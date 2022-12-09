using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Runtime;

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    internal class TestWorkflowOrchestratorProvider : IWorkflowOrchestratorProvider
    {
        private Dictionary<string, WorkflowOrchestrator> workflowOrchestrators = new Dictionary<string, WorkflowOrchestrator>();

        public WorkflowOrchestrator GetWorkflowOrchestrator(string workflowName)
        {
            return workflowOrchestrators[workflowName];
        }

        public void SetWorkflowOrchestrator(string workflowName, WorkflowOrchestrator workflowOrchestrator)
        {
            workflowOrchestrators[workflowName] = workflowOrchestrator;
        }
    }
}
