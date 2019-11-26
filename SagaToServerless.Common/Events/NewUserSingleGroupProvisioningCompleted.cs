using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using System;

namespace SagaToServerless.Common.Events
{
    public class NewUserSingleGroupProvisioningCompleted : ICommand
    {
        public NewUserSingleGroupProvisioningCompleted(
            Guid correlationId, 
            Guid assignToGroupId, 
            Guid assignedToGroupId, 
            UserModel user, 
            string operatorEmail, 
            bool userCreated, 
            string reason = "")
        {
            CorrelationId = correlationId;
            AssignToGroupId = assignToGroupId;
            AssignedToGroupId = assignedToGroupId;
            User = user;
            UserCreated = userCreated;
            OperatorEmail = operatorEmail;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public Guid AssignToGroupId { get; set; }
        public Guid AssignedToGroupId { get; set; }
        public UserModel User { get; set; }
        public bool UserCreated { get; set; }
        public string OperatorEmail { get; set; }
        public string Reason { get; set; }
    }
}
