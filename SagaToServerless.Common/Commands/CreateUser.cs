using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class CreateUser : ICommand
    {
        public CreateUser(
            Guid correlationId,
            UserModel user,
            List<Guid> groupIds,
            string createdBy)
        {
            CorrelationId = correlationId;
            User = user;
            CreatedBy = createdBy;
            GroupIds = groupIds;
        }

        public Guid CorrelationId { get; set; }
        public UserModel User { get; set; }
        public List<Guid> GroupIds { get; set; }
        public string CreatedBy { get; set; }
    }
}
