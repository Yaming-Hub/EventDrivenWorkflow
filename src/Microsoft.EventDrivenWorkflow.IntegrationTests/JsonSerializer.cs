using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Persistence;
using System.Text.Json;

namespace Microsoft.EventDrivenWorkflow.Core.IntegrationTests
{
    internal class TestJsonSerializer : ISerializer
    {
        public object Deserialize(byte[] bytes, Type type)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return System.Text.Json.JsonSerializer.Deserialize(ms, type);
            }
        }

        public byte[] Serialize(object value)
        {
            using (MemoryStream ms = new MemoryStream(capacity: 1024))
            {
                System.Text.Json.JsonSerializer.Serialize(ms, value);
                return ms.ToArray();
            }
        }
    }
}
