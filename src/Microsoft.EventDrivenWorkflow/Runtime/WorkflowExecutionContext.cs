﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowExecutionContext.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This class defines the workflow execution context.
    /// </summary>
    public class WorkflowExecutionContext
    {
        /// <summary>
        /// Gets the workflow name.
        /// </summary>
        public string WorkflowName { get; init; }

        /// <summary>
        /// Gets the workflow version.
        /// </summary>
        public string WorkflowVersion { get; init; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public string PartitionKey { get; init; }

        /// <summary>
        /// Gets the workflow id.
        /// </summary>
        public Guid WorkflowId { get; init; }

        /// <summary>
        /// Gets the workflow start time.
        /// </summary>
        public DateTime WorkflowStartDateTime { get; init; }
    }
}