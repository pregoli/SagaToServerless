using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class ProvisionNewUserMultipleGroups : ICommand
    {
        public ProvisionNewUserMultipleGroups()
        {
            GroupIds = new List<Guid>();
        }

        public Guid CorrelationId { get; set; }
        public string OperatorEmail { get; set; }
        public UserModel User { get; set; }
        public List<Guid> GroupIds { get; set; }
    }
}
