using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class SendApproval : ICommand
    {
        public SendApproval(
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
