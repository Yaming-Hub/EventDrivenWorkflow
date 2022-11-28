// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowType.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This enum defines different type of workflows.
    /// </summary>
    public enum WorkflowType
    {
        /// <summary>
        /// In static workflow, each activity must publish all defined output events. That means, static workflow will
        /// not support branching or looping behaviors in the workflow. Static workflow has clear terminating activities
        /// defined and the workflow completes when all terminating activites are completed. During workflow execution
        /// activity can only be executed once.
        /// </summary>
        Static,

        /// <summary>
        /// In dynamic workflow, the activity may publish a subset of defined output events or even no output events. 
        /// This will allow the workflow to support advanced branching or looping behaviors. There is no deterministic
        /// terminating activity in dynamic workflow as any workflow may not publish output events. The workflow will
        /// be considered as complete when there is no more event being published. It's possible that some activities 
        /// may not run after workflow completes.
        /// </summary>
        Dynamic
    }
}
