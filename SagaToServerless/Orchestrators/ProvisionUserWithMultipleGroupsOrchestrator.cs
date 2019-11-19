using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Models;
using SagaToServerless.Durable.Dto;
using SagaToServerless.Durable.Extensions;

namespace SagaToServerless.Durable.Orchestrators
{
    public class ProvisionUserWithMultipleGroupsOrchestrator
    {
        [FunctionName(Constants.FunctionNames.Orchestrator.StartProvisionUserWithMultipleGroupsOrchestrator)]
        public async Task<List<WorkflowStepResult>> StartProvisionUserWithMultipleGroupsOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            var input = context.GetInput<ProvisionNewUserMultipleGroups>();

            if (!context.IsReplaying)
                logger.LogInformation($"Started User deprovisioning workflow with InstanceId: {input.CorrelationId.ToString()}");

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3);

            var outputs = new List<WorkflowStepResult>();

            var createdUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.CreateUser, retryOptions, (input.OperatorEmail, input.User));
            outputs.Add(createdUserResult);

            if (outputs.FirstOrDefault().Successfull)
            {
                var tasks = new Task<WorkflowStepResult>[input.GroupIds.Count];
                for (int i = 0; i < input.GroupIds.Count; i++)
                    tasks[i] = context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.AssignUserToGroup, retryOptions, new AssignUserToGroupModel(input.GroupIds[i], createdUserResult.OutputId));

                var paralellOutputs = await Task.WhenAll(tasks);

                outputs.AddRange(paralellOutputs);
            }

            await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                        from: string.Empty,
                        to: input.OperatorEmail,
                        subject: $"Provisioning User {input.User.FirstName} {input.User.FirstName} with Groups {string.Join(',', input.GroupIds)}",
                        htmlBody: outputs.ToProvisioningUserMailBody()));

            return outputs;
        }
    }
}
