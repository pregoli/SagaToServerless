using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserAssignedToGroupsCompleted : ICommand
    {
        public UserAssignedToGroupsCompleted(
            Guid correlationId, 
            List<Guid> assignedGroupIds, 
            bool successfull = true, 
            string reason = "")
        {
            CorrelationId = correlationId;
            AssignedGroupIds = assignedGroupIds;
            Successfull = successfull;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> AssignedGroupIds { get; set; }
        public bool Successfull { get; set; }
        public string Reason { get; set; }
    }
}
