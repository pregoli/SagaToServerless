using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class NotificationSentSuccessfully : ICommand
    {
        public NotificationSentSuccessfully(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }
    }
}

