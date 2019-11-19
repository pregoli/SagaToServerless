using SagaToServerless.Common.Commands;
using System;

namespace SagaToServerless.Common.Events
{
    public class NotificationSentUnsuccessfully : ICommand
    {
        public NotificationSentUnsuccessfully(
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

