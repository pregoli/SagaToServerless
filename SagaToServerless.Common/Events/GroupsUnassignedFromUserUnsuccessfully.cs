using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;

namespace SagaToServerless.Common.Events
{
    public class GroupsUnassignedFromUserUnsuccessfully : ICommand
    {
        public GroupsUnassignedFromUserUnsuccessfully(
            Guid correlationId,
            List<Guid> unassignedGroupIds,
            string reason)
        {
            CorrelationId = correlationId;
            UnassignedGroupIds = unassignedGroupIds;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> UnassignedGroupIds { get; set; }
        public string Reason { get; set; }
    }
}
