using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Environment
{
    public class LogActivityFactory : IActivityFactory
    {
        public IActivity CreateActivity(string name)
        {
            return new LogActivity();
        }

        public IAsyncActivity CreateAsyncActivity(string name)
        {
            throw new NotImplementedException();
        }
    }
}
