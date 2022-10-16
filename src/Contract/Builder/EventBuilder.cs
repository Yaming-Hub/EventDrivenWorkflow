using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Definitions;

namespace Microsoft.EventDrivenWorkflow.Contract.Builder
{
    public class EventBuilder
    {
        internal EventBuilder(string name, Type payloadType)
        {
            if (StringConstraint.Name.IsValid(name, out string reason))
            {
                throw new ArgumentException($"Event name {reason}", paramName: nameof(name));
            }

            this.Name = name;
            this.PayloadType = payloadType;
        }

        internal string Name { get; }

        internal Type PayloadType { get; }

        internal EventDefinition Build()
        {
            return new EventDefinition
            {
                Name = this.Name,
                PayloadType = this.PayloadType,
            };
        }
    }
}
