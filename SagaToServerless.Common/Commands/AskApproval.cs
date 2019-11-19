using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class AskApproval : ICommand
    {
        public AskApproval(
            Guid correlationId,
            Guid groupId,
            string operatorEmail,
            UserModel user)
        {
            CorrelationId = correlationId;
            GroupId = groupId;
            OperatorEmail = operatorEmail;
            User = user;
        }

        public Guid CorrelationId { get; set; }
        public Guid GroupId { get; set; }
        public string OperatorEmail { get; set; }
        public UserModel User { get; set; }
    }
}
