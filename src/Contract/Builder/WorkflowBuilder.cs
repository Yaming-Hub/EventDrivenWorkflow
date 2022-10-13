using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Builder
{
    public class WorkflowBuilder
    {
        public void RegisterEvent(string name)
        {
            
        }

        public void RegisterEvent<T>(string name)
        {

        }

        public ActivityBuilder CreateActivity(string name)
        {
            throw new NotImplementedException();
        }

        public WorkflowDefinition Build()
        {
            throw new NotImplementedException();
        }
    }
}
