using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserCreatedSuccessfully : ICommand
    {
        public UserCreatedSuccessfully(
            Guid correlationId,
            Guid userId)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }

        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}
