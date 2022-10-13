using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract;

namespace Microsoft.EventDrivenWorkflow.Core
{
    internal class WorkflowExecutionContext
    {
        public WorkflowDefinition WorkflowDefinition { get; }

        public WorkflowExecutionContext(WorkflowDefinition workflowDefinition, Guid executionId)
        {
            WorkflowDefinition = workflowDefinition;
            ExecutionId = executionId;
        }

        public Guid ExecutionId { get; }
    }
}
