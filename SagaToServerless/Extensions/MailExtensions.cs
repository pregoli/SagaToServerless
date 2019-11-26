using SagaToServerless.Common.Models;
using SagaToServerless.Durable.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SagaToServerless.Durable.Extensions
{
    public static class MailExtensions
    {
        public static string ToProvisioningUserMailBody(this List<WorkflowStepResult> outputs, UserModel user)
        {
            var mailBody = new StringBuilder();

            if (outputs[0].Successfull)
                mailBody.Append($"User: <strong style='color:green;'>{user.FirstName} {user.LastName}</strong> created Successfully.</br>");
            else
            {
                mailBody.Append($"User: <strong style='color:red;'>{user.FirstName} {user.LastName}</strong>");
                mailBody.Append($" cannot be created for the following reason: </br>");
                mailBody.Append($"<strong style='color:red;'>{outputs[0].Reason}</strong>");
                return mailBody.ToString();
            }

            var successfullGroupIds = outputs.Where(x => x.ActionName == "AssignUserToGroup" && x.Successfull).Select(x => x.OutputId);
            if (successfullGroupIds != null && successfullGroupIds.Any())
            {
                mailBody.AppendLine($"User successfully assigned to groups:</br>");
                foreach (var successfullGroupId in successfullGroupIds)
                    mailBody.Append($"<strong style='color:green;'>{successfullGroupId}</strong></br>");
            }
            else
                mailBody.Append($"<strong style='color:red;'>No groups will be assigned.</strong>");

            return mailBody.ToString();
        }

        public static string ToProvisioningUserMailBodyWithApproval(this List<WorkflowStepResult> outputs, UserModel user, bool approved)
        {
            var mailBody = new StringBuilder();

            if (!approved)
            {
                mailBody.Append($"the provisioning for user <strong style='color:blue;'>{user.UserName}</strong> has been <strong style='color:red;'>REJECTED</strong></br>");
                return mailBody.ToString();
            }

            if (outputs[1].Successfull)
                mailBody.Append($"User: <strong style='color:green;'>{user.FirstName} {user.LastName}</strong> created Successfully.</br>");
            else
            {
                mailBody.Append($"User: <strong style='color:red;'>{user.FirstName} {user.LastName}</strong>");
                mailBody.Append($" cannot be created for the following reason: </br>");
                mailBody.Append($"<strong style='color:red;'>{outputs[1].Reason}</strong>");
                return mailBody.ToString();
            }

            var successfullGroupIds = outputs.Where(x => x.ActionName == "AssignUserToGroup" && x.Successfull).Select(x => x.OutputId);
            if (successfullGroupIds != null && successfullGroupIds.Any())
            {
                mailBody.AppendLine($"User successfully assigned to groups:</br>");
                foreach (var successfullGroupId in successfullGroupIds)
                    mailBody.Append($"<strong style='color:green;'>{successfullGroupId}</strong></br>");
            }
            else
                mailBody.Append($"<strong style='color:red;'>No groups will be assigned.</strong>");

            return mailBody.ToString();
        }
        
        public static string ToExpiredProvisioningUserMailBody(this string userName)
        {
            var mailBody = new StringBuilder();
            mailBody.Append($"the provisioning for user <strong style='color:blue;'>{userName}</strong> has <strong style='color:red;'>EXPIRED</strong></br>");

            return mailBody.ToString();
        }

        public static string ToApprovalMailBody(this UserModel user, Guid instanceId)
        {
            return $@"
                I need to create the following user:</br>
                FirstName: <stron>{user.FirstName}</stron></br>
                LastName: <stron>{user.LastName}</stron></br>
                UserName: <stron>{user.UserName}</stron></br>
                </br>
                </br>
                <a style='font-size:20px; color: blue;' href='http://localhost:7071/api/approval?instanceid={instanceId}&response=approved'>Approve</a>
                <br>
                <a style='font-size:20px; color: red;' href='http://localhost:7071/api/approval?instanceid={instanceId}&response=rejected'>Reject</a>";
        }
    }
}
