using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventDrivenWorkflow.Definitions;

namespace EventDrivenWorkflow.IntegrationTests.Environment
{
    public class LogActivityFactory : IExecutableFactory
    {
        private WorkflowDefinition workflowDefinition;

        public LogActivityFactory(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;
        }

        public IExecutable CreateExecutable(string name)
        {
            return new LogActivity(this.workflowDefinition);
        }

        public IAsyncExecutable CreateAsyncExecutable(string name)
        {
            throw new NotImplementedException();
        }
    }
}
