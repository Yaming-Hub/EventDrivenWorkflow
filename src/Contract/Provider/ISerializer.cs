using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract.Provider
{
    public interface ISerializer
    {
        byte[] Serialize(object value);

        object Deserialize(byte[] bytes, Type type);
    }
}
