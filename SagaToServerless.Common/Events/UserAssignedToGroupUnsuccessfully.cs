﻿using SagaToServerless.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Events
{
    public class UserAssignedToGroupUnsuccessfully : ICommand
    {
        public UserAssignedToGroupUnsuccessfully(
            Guid correlationId,
            Guid groupId,
            string reason)
        {
            CorrelationId = correlationId;
            GroupId = groupId;
            Reason = reason;
        }

        public Guid CorrelationId { get; set; }
        public Guid GroupId { get; set; }
        public string Reason { get; set; }
    }
}
