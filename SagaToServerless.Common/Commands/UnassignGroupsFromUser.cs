using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class UnassignGroupsFromUser : ICommand
    {
        public UnassignGroupsFromUser(
            Guid correlationId,
            Guid userId,
            List<Guid> groupIds)
        {
            CorrelationId = correlationId;
            UserId = userId;
            GroupIds = groupIds;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> GroupIds { get; set; }
        public Guid UserId { get; set; }
    }
}
