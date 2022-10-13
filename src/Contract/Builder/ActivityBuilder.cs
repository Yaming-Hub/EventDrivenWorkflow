using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Builder
{
    public class ActivityBuilder
    {
        public ActivityBuilder Subscribe(string eventName)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Subscribe<T>(string eventName)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Publish(string eventName)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Publish(params string[] eventNames)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Publish<T>(string eventName)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Publish<T1, T2>(string eventName1, string eventName2)
        {
            throw new NotImplementedException();
        }

        public ActivityBuilder Publish<T1, T2, T3>(string eventName1, string eventName2, string eventName3)
        {
            throw new NotImplementedException();
        }
    }
}
