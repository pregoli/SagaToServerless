using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class SendApproval : ICommand
    {
        public SendApproval(
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
