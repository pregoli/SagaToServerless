using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class NewUserSingleGroupProvisioningUnsuccessfully : ICommand
    {
        public NewUserSingleGroupProvisioningUnsuccessfully(
            Guid correlationId, 
            Guid assignToGroupId, 
            Guid assignedToGroupId, 
            UserModel user, 
            string operatorEmail, 
            string reason)
        {
            CorrelationId = correlationId;
            AssignToGroupId = assignToGroupId;
            AssignedToGroupId = assignedToGroupId;
            User = user;
            OperatorEmail = operatorEmail;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public Guid AssignToGroupId { get; set; }
        public Guid AssignedToGroupId { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
        public string Reason { get; set; }
    }
}
