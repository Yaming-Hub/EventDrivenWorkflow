﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventDrivenWorkflow.Contract
{
    public class WorkflowExecutionInfo
    {
        public string WorkflowName { get; init; }

        public string WorkflowVersion { get; init; }

        public string PartitionKey { get; init; }

        public Guid WorkflowId { get; init; }

        public DateTime WorkflowStartDateTime { get; init; }
    }
}
