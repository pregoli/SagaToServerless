using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserCreatedUnsuccessfully : ICommand
    {
        public UserCreatedUnsuccessfully(
            Guid correlationId,
            string reason)
        {
            CorrelationId = correlationId;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }
}
