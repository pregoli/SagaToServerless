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
                logger.LogInformation($"Started User provisioning approval workflow with InstanceId: {input.CorrelationId}");

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromMinutes(5),
                maxNumberOfAttempts: 3);

            await context.CallActivityWithRetryAsync(Constants.FunctionNames.Activity.AskUserCreationApproval, retryOptions, input);

            using (var timeoutCts = new CancellationTokenSource())
            {
                var expiration = context.CurrentUtcDateTime.AddMinutes(5);
                var timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                //This external event will be triggered on mail click hopefully... :)
                var approvalResponse = context.WaitForExternalEvent<bool>("ReceiveApprovalResponse");
                var winner = await Task.WhenAny(approvalResponse, timeoutTask);

                if (winner == approvalResponse)
                {
                    if (approvalResponse.Result)
                    {
                        var createdUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.CreateUser, retryOptions, (input.OperatorEmail, input.User));
                        outputs.Add(createdUserResult);

                        if (outputs[0].Successfull)
                            outputs.Add(await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.AssignUserToGroup, retryOptions, new AssignUserToGroupModel(input.GroupId, createdUserResult.OutputId)));
                    }

                    await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                                from: string.Empty,
                                to: input.OperatorEmail,
                                subject: $"Provisioning User {input.User.FirstName} {input.User.FirstName} with Group {input.GroupId}",
                                htmlBody: outputs.ToProvisioningUserMailBody()));

                    return outputs;
                }

                if (!timeoutTask.IsCompleted)
                    timeoutCts.Cancel();
                else
                {
                    context.SetOutput("Expired");

                    await context.CallActivityWithRetryAsync(
                            Constants.FunctionNames.Activity.SendEmail,
                            retryOptions,
                            new MailItemModel(
                                from: string.Empty,
                                to: input.OperatorEmail,
                                subject: $"Provisioning User {input.User.FirstName} {input.User.FirstName} with Group {input.GroupId}",
                                htmlBody: outputs.ToProvisioningUserMailBody()));
                }

                return outputs;
            }
        }
    }
}
