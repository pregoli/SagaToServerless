using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserAssignedToGroupSuccessfully : ICommand
    {
        public UserAssignedToGroupSuccessfully(
            Guid correlationId,
            Guid outputId)
        {
            CorrelationId = correlationId;
            OutputId = outputId;
        }

        public Guid CorrelationId { get; set; }
        public Guid OutputId { get; set; }
    }
}
