using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class AssignUserToGroups : ICommand
    {
        public AssignUserToGroups(
            Guid correlationId,
            List<Guid> groupIds,
            Guid userId)
        {
            CorrelationId = correlationId;
            GroupIds = groupIds;
            UserId = userId;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> GroupIds { get; set; }
        public Guid UserId { get; set; }
    }
}
