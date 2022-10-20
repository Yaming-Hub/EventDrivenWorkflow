// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowExecutionOptions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime
{
    /// <summary>
    /// This class defines options of workflow orchestration.
    /// </summary>
    public sealed class WorkflowExecutionOptions
    {
        /// <summary>
        /// The default workflow execution options.
        /// </summary>
        public static readonly WorkflowExecutionOptions Default = new WorkflowExecutionOptions();

        /// <summary>
        /// Gets a value indicates whether to ignore the event if it's published by previous version 
        /// of the workflow activity. Set this value to true if there is a breaking change in the workflow
        /// definition to avoid unexpected behavior. The previous workflow may timeout as events are
        /// ignored.
        /// </summary>
        public bool AbandonOnVersionChange { get; init; } = false;

        /// <summary>
        /// Gets a value indicates whether to track finish and timeout for the workflow.
        /// </summary>
        public bool TrackProgress { get; init; } = false;
    }
}
