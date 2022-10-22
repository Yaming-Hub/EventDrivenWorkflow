// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlModel.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the data model used control workflow.
    /// </summary>
    public sealed class ControlModel
    {
        public ControlModel()
        {

        }

        /// <summary>
        /// Gets the control operation.
        /// </summary>
        public ControlOperation Operation { get; init; }

        /// <summary>
        /// Gets the name of the target activity.
        /// </summary>
        public string TargetActivityName { get; init; }

        public ActivityExecutionContext ActivityExecutionContext { get; init; }

        /// <summary>
        /// Gets the event model.
        /// </summary>
        public EventModel Event { get; init; }
    }
}
