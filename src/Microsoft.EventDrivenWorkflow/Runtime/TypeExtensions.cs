using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    internal static class TypeExtensions
    {
        public static string GetDisplayName(this Type type)
        {
            return type == null ? "<null>" : type.FullName;
        }
    }
}
