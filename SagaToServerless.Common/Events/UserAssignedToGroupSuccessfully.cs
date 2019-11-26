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
            Guid assignedGroupId)
        {
            CorrelationId = correlationId;
            AssignedGroupId = assignedGroupId;
        }

        public Guid CorrelationId { get; set; }
        public Guid AssignedGroupId { get; set; }
    }
}
