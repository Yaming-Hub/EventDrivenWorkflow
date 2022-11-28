// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Payload.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace EventDrivenWorkflow.Runtime.Data
{
    /// <summary>
    /// This class defines the payload.
    /// </summary>
    public sealed class Payload
    {
        /// <summary>
        /// Gets the full name CLR type.
        /// </summary>
        public string TypeName { get; init; }

        /// <summary>
        /// Gets the payload body in bytes.
        /// </summary>
        public byte[] Body { get; init; }
    }
}
