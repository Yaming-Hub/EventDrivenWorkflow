﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EventDrivenWorkflow.Contract.Persistence;

namespace Microsoft.EventDrivenWorkflow.Core.Model
{
    public class EventEntity : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; init; }

        public string PayloadType { get; init; }

        public byte[] Payload { get; init; }

        public TimeSpan DelayDuration { get; init; }

        public string ETag { get; set; }

        public DateTime ExpireDateTime { get; set; }
    }
}