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

            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3);

            var outputs = new List<WorkflowStepResult>();

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
                        (createUserResult.OutputId, assignUserToGroupResult.OutputId));

                    outputs.Add(unassignGroupFromUserResult);
                }
            }

            await context.CallActivityWithRetryAsync<WorkflowStepResult>(Constants.FunctionNames.Activity.SendEmail, retryOptions, new MailItemModel(
                                    from: string.Empty,
                                    to: input.OperatorEmail,
                                    subject: $"Provisioning User {input.User.FirstName} {input.User.LastName} with Group {input.GroupId}",
                                    htmlBody: outputs.ToProvisioningUserMailBody(input.User)));

            return outputs;
        }
    }
}
