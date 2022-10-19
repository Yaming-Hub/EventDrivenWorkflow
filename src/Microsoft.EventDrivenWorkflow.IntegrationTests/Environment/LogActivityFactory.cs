using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Definitions;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class LogActivityFactory : IActivityFactory
    {
        private WorkflowDefinition workflowDefinition;

        public LogActivityFactory(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;
        }

        public IActivity CreateActivity(string name)
        {
            return new LogActivity(this.workflowDefinition);
        }

        public IAsyncActivity CreateAsyncActivity(string name)
        {
            throw new NotImplementedException();
        }
    }
}
