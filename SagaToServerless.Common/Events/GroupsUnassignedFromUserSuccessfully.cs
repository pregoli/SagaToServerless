using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class GroupsUnassignedFromUserSuccessfully : ICommand
    {
        public GroupsUnassignedFromUserSuccessfully(
            Guid correlationId,
            List<Guid> groupIds)
        {
            CorrelationId = correlationId;
            UnassignedGroupIds = groupIds;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> UnassignedGroupIds { get; set; }
    }
}
