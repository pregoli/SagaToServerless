using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class ApprovalReceived : ICommand
    {
        public ApprovalReceived(
            Guid correlationId,
            bool approved)
        {
            CorrelationId = correlationId;
            Approved = approved;
        }

        public Guid CorrelationId { get; set; }
        public bool Approved { get; set; }
    }
}
