using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using SagaToServerless.Common;
using System.Threading.Tasks;
using SagaToServerless.Common.Commands;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common.Models;
using SagaToServerless.Durable.Dto;
using SagaToServerless.Durable.Extensions;

namespace SagaToServerless.Durable.Orchestrators
{
    public class ProvisionUserWithSingleGroupOrchestrator
    {
        [FunctionName(Constants.FunctionNames.Orchestrator.StartProvisionUserWithSingleGroupOrchestrator)]
        public async Task<List<WorkflowStepResult>> StartProvisionUserWithSingleGroupOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            var input = context.GetInput<ProvisionNewUserSingleGroup>();

            if (!context.IsReplaying)
                logger.LogInformation($"Started User provisioning workflow with InstanceId: {input.CorrelationId.ToString()}");

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3);

            var outputs = new List<WorkflowStepResult>();

            var createdUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.CreateUser, retryOptions, (input.OperatorEmail, input.User));
            outputs.Add(createdUserResult);

            if (outputs[0].Successfull)
                outputs.Add(await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.AssignUserToGroup, retryOptions, new AssignUserToGroupModel(input.GroupId, createdUserResult.OutputId)));

            await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                                    from: string.Empty,
                                    to: input.OperatorEmail,
                                    subject: $"Provisioning User {input.User.FirstName} {input.User.LastName} with Group {input.GroupId}",
                                    htmlBody: outputs.ToProvisioningUserMailBody()));

            return outputs;
        }
    }
}
