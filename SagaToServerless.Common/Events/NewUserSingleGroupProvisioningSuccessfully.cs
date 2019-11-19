using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class NewUserSingleGroupProvisioningSuccessfully : ICommand
    {
        public NewUserSingleGroupProvisioningSuccessfully(
            Guid correlationId, 
            Guid assignToGroupId, 
            Guid assignedToGroupId, 
            UserModel user, 
            string operatorEmail)
        {
            CorrelationId = correlationId;
            AssignToGroupId = assignToGroupId;
            AssignedToGroupId = assignedToGroupId;
            User = user;
            OperatorEmail = operatorEmail;
        }

        public Guid CorrelationId { get; set; }
        public Guid AssignToGroupId { get; set; }
        public Guid AssignedToGroupId { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
    }
}
