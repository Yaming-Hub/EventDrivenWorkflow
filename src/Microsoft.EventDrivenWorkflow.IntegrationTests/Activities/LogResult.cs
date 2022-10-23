// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogResult.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Activities
{
    using System.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    internal sealed class LogResult<TResult> : IExecutable
    {
        private readonly string resultEventName;

        public LogResult(string resultEventName = "result") 
        {
            this.resultEventName = resultEventName;
        }

        public Task Execute(
           ExecutionContext context,
           IEventRetriever eventRetriever,
           IEventPublisher eventPublisher,
           CancellationToken cancellationToken)
        {
            var result = eventRetriever.GetEvent<TResult>(this.resultEventName).Payload;
            Trace.WriteLine($"[Forward.Execute] Result={result} Path={context.GetPath()}");
            return Task.CompletedTask;
        }
    }
}
