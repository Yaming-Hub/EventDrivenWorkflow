// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkflowLink.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Definitions
{
    /// <summary>
    /// This class defines
    /// </summary>
    public sealed class WorkflowLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowLink"/> class.
        /// </summary>
        internal WorkflowLink()
        {
        }

        /// <summary>
        /// Gets the source activity of the link.
        /// </summary>
        public ActivityDefinition Source { get; init; }

        /// <summary>
        /// Gets the event of the link.
        /// </summary>
        public EventDefinition Event { get; init; }

        /// <summary>
        /// Gets the target activity of the link.
        /// </summary>
        public ActivityDefinition Target { get; init; }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Source, this.Event, this.Target);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is WorkflowLink that))
            {
                return false;
            }

            return this.Source == that.Source && this.Event == that.Event && this.Target == that.Target;
        }
    }
}
