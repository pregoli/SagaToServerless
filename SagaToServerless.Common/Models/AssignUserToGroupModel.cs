using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Models
{
    public class AssignUserToGroupModel
    {
        public AssignUserToGroupModel(
            Guid groupId,
            Guid userId)
        {
            GroupId = groupId;
            UserId = userId;
        }

        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
    }
}
