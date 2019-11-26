using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class NewUserSingleGroupCreationRejected : ICommand
    {
        public NewUserSingleGroupCreationRejected(
            Guid correlationId,
            UserModel user,
            string operatorEmail)
        {
            CorrelationId = correlationId;
            User = user;
            OperatorEmail = operatorEmail;
        }

        public Guid CorrelationId { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
    }
}
