using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserAssignedToGroupUnsuccessfully : ICommand
    {
        public UserAssignedToGroupUnsuccessfully(
            Guid correlationId,
            Guid outputId,
            string reason)
        {
            CorrelationId = correlationId;
            OutputId = outputId;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public Guid OutputId { get; set; }
        public string Reason { get; set; }
    }
}
