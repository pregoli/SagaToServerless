using SagaToServerless.Durable.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SagaToServerless.Durable.Extensions
{
    public static class WorkflowStepResultExtensions
    {
        public static string ToProvisioningUserMailBody(this List<WorkflowStepResult> outputs)
        {
            var mailBody = new StringBuilder();
            mailBody.Append($"UserCreated: <strong style='color:green;'>{outputs[0].Successfull}</strong></br>");

            var successfullGroupIds = outputs.Skip(1).Where(x => x.Successfull && x.OutputId != Guid.Empty).Select(x => x.OutputId);
            if (successfullGroupIds.Any())
            {
                mailBody.AppendLine($"User successfully assigned to groups:</br>");
                foreach (var successfullGroupId in successfullGroupIds)
                    mailBody.Append($"<strong style='color:green;'>{successfullGroupId}</strong></br>");
            }
            else
                mailBody.Append($"<strong style='color:red;'>No groups will be assigned.</strong>");

            return mailBody.ToString();
        }
    }
}
