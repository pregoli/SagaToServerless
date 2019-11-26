using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using SagaToServerless.Durable.Dto;
using SagaToServerless.Durable.Extensions;

namespace SagaToServerless.Durable.Orchestrators
{
    public class ProvisionUserOrchestratorApproval
    {
        [FunctionName(Constants.FunctionNames.Orchestrator.ExecuteUserProvisioninApprovalgWorkflow)]
        public async Task<List<WorkflowStepResult>> ExecuteUserProvisioninApprovalgWorkflow(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            var input = context.GetInput<ProvisionNewUserSingleGroup>();

            var outputs = new List<WorkflowStepResult>();

            if (!context.IsReplaying)
                logger.LogInformation($"Executing User provisioning approval workflow with InstanceId: {input.CorrelationId}");

            var retryOptions = new RetryOptions(TimeSpan.FromMinutes(5), 3);

            await context.CallActivityWithRetryAsync(Constants.FunctionNames.Activity.AskUserCreationApproval, retryOptions, input);

                var approvalResult = await context.WaitForExternalEvent<bool>("ReceiveApprovalResponse");
                if (approvalResult)
                {
                    outputs.Add(new WorkflowStepResult(Constants.FunctionNames.Activity.AskUserCreationApproval, Guid.Empty, approvalResult, "approved"));

                    var createUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.CreateUser, retryOptions, (input.OperatorEmail, input.User, new List<Guid> { input.GroupId }));
                    outputs.Add(createUserResult);

                    if (createUserResult.Successfull)
                    {
                        var assignUserToGroupResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(
                            Constants.FunctionNames.Activity.AssignUserToGroup,
                            retryOptions,
                            new AssignUserToGroupModel(input.GroupId, createUserResult.OutputId));

                        outputs.Add(assignUserToGroupResult);

                        if (!assignUserToGroupResult.Successfull)
                        {
                            var unassignGroupFromUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(
                                Constants.FunctionNames.Activity.UnassignGroupFromUser,
                                retryOptions,
                                (createUserResult.OutputId, assignUserToGroupResult.OutputId.ToString()));

                            outputs.Add(unassignGroupFromUserResult);
                        }
                    }
                }
                else
                    outputs.Add(new WorkflowStepResult(Constants.FunctionNames.Activity.AskUserCreationApproval, Guid.Empty, approvalResult, "rejected"));

                await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                            from: string.Empty,
                            to: input.OperatorEmail,
                            subject: $"Provisioning User {input.User.FirstName} {input.User.FirstName} with Group {input.GroupId}",
                            htmlBody: outputs.ToProvisioningUserMailBodyWithApproval(input.User, approvalResult)));

                return outputs;
        }
    }
}
