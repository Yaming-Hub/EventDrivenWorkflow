// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogResult.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EventDrivenWorkflow.IntegrationTests.Activities
{
    using System.Diagnostics;
    using Microsoft.EventDrivenWorkflow.Runtime.Data;

    internal sealed class SendMessage<TMessage> : IExecutable
    {
        private readonly string eventName;
        private readonly TMessage message;

        public SendMessage(TMessage message, string eventName = "message")
        {
            this.eventName = eventName;
            this.message = message;
        }

        public Task Execute(
           ExecutionContext context,
           IEventRetriever eventRetriever,
           IEventPublisher eventPublisher,
           CancellationToken cancellationToken)
        {
            Trace.WriteLine($"[SendMessage.Execute] EventName={this.eventName} Message={this.message} Path={context.GetPath()}");
            eventPublisher.PublishEvent(this.eventName, this.message);
            return Task.CompletedTask;
        }
    }
}
