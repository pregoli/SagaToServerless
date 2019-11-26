using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SagaToServerless.SagaPattern.Extensions
{
    public static class MailExtensions
    {
        public static string ToMailSubject(this UserModel user, List<Guid> groupIds)
        {
            return $"Provisioning User {user.FirstName} {user.LastName} with Groups {string.Join(',', groupIds)}";
        }

        public static string ToMailBody(this UserModel user, bool userCreated, List<Guid> outputIds, string error = "")
        {
            var mailBody = new StringBuilder();

            if (!userCreated)
            {
                mailBody.Append($"User: <strong style='color:red;'>{user.FirstName} {user.LastName}</strong>");
                mailBody.Append($" cannot be created for the following reason: </br>");
                mailBody.Append($"<strong style='color:red;'>{error}</strong>");
                return mailBody.ToString();
            }

            mailBody.Append($"User: <strong style='color:green;'>{user.FirstName} {user.LastName}</strong> created Successfully.</br>");

            var validOutputIds = outputIds.Where(x => x != Guid.Empty);
            if (validOutputIds.Any())
            {
                mailBody.AppendLine($"User successfully assigned to groups:</br>");
                foreach (var validOutputId in validOutputIds)
                    mailBody.Append($"<strong style='color:green;'>{validOutputId}</strong></br>");
            }
            else
                mailBody.Append($"<strong style='color:red;'>No groups will be assigned.</strong>");

            if(!string.IsNullOrEmpty(error))
                mailBody.Append($"</br>ERROR: <strong style='color:red;'>{error}</strong>");

            return mailBody.ToString();
        }

        public static string ToApprovalRejectedMailBody(this UserModel user, string reason)
        {
            var mailBody = new StringBuilder();
            mailBody.Append($"User: <strong style='color:red;'>{user.FirstName} {user.LastName}</strong> will be not created.</br>");
            mailBody.Append($"<strong style='color:red;'>REQUEST REJECTED - {reason}</strong>");
            return mailBody.ToString();
        }
    }
}
