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
            bool approved,
            string reason = "")
        {
            CorrelationId = correlationId;
            Approved = approved;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public bool Approved { get; set; }
        public string Reason { get; set; }
    }
}
