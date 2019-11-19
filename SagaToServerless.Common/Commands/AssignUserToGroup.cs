using System;

namespace SagaToServerless.Common.Commands
{
    public class AssignUserToGroup : ICommand
    {
        public AssignUserToGroup(
            Guid correlationId, 
            Guid groupId, 
            Guid userId)
        {
            CorrelationId = correlationId;
            GroupId = groupId;
            UserId = userId;
        }

        public Guid CorrelationId { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
    }
}
