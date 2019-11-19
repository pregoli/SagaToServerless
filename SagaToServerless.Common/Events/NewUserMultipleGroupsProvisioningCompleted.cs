using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class NewUserMultipleGroupsProvisioningCompleted : ICommand
    {
        public NewUserMultipleGroupsProvisioningCompleted(
            Guid correlationId, 
            List<Guid> assignToGroupIds, 
            List<Guid> assignedToGroupIds, 
            UserModel user, 
            string operatorEmail, 
            string reason)
        {
            CorrelationId = correlationId;
            AssignToGroupIds = assignToGroupIds;
            AssignedToGroupIds = assignedToGroupIds;
            User = user;
            OperatorEmail = operatorEmail;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public List<Guid> AssignToGroupIds { get; set; }
        public List<Guid> AssignedToGroupIds { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
        public string Reason { get; set; }
    }
}
