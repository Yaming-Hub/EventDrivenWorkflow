// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlModel.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the data model used control workflow.
    /// </summary>
    public class ControlModel
    {
        public ControlModel()
        {

        }

        /// <summary>
        /// Gets the control operation.
        /// </summary>
        public ControlOperation Operation { get; init; }

    }
}
