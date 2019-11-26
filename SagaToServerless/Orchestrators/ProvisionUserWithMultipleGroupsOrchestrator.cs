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
                logger.LogInformation($"Executing User provisioning workflow with InstanceId: {input.CorrelationId.ToString()}");

            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3);

            var outputs = new List<WorkflowStepResult>();

            var createUserResult = await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.CreateUser, retryOptions, (input.OperatorEmail, input.User, input.GroupIds));
            outputs.Add(createUserResult);

            if (createUserResult.Successfull)
            {
                var assignUserToGroupTasks = new Task<WorkflowStepResult>[input.GroupIds.Count];
                for (int i = 0; i < input.GroupIds.Count; i++)
                    assignUserToGroupTasks[i] = context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.AssignUserToGroup, retryOptions, new AssignUserToGroupModel(input.GroupIds[i], createUserResult.OutputId));

                var paralellOutputs = await Task.WhenAll(assignUserToGroupTasks);
                outputs.AddRange(paralellOutputs);


                var unassignedGroups = paralellOutputs.Where(x => !x.Successfull).ToList();
                if (unassignedGroups.Any())
                {
                    var unassignGroupFromUserTasks = new Task<WorkflowStepResult>[unassignedGroups.Count];
                    for (int i = 0; i < unassignedGroups.Count; i++)
                        unassignGroupFromUserTasks[i] = context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.UnassignGroupFromUser, retryOptions, (createUserResult.OutputId, unassignedGroups[i].OutputId.ToString()));
                }
            }

            await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                        from: string.Empty,
                        to: input.OperatorEmail,
                        subject: $"Provisioning User {input.User.FirstName} {input.User.FirstName} with Groups {string.Join(',', input.GroupIds)}",
                        htmlBody: outputs.ToProvisioningUserMailBody(input.User)));

            return outputs;
        }
    }
}
