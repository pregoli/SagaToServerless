using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class ProvisionNewUserSingleGroupWithApproval : ICommand
    {
        public Guid CorrelationId { get; set; }
        public string OperatorEmail { get; set; }
        public UserModel User { get; set; }
        public Guid GroupId { get; set; }
    }
}
