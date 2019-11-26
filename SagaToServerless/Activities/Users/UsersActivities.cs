using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common.Models;
using SagaToServerless.Services;
using System;
using System.Collections.Generic;
using SagaToServerless.Common;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SagaToServerless.Common.Commands;
using SagaToServerless.Durable.Dto;
using SagaToServerless.Durable.Extensions;

namespace SagaToServerless.Durable.Activities.Users
{
    public class UsersActivities
    {
        private readonly IUserService _userService;
        private readonly ISendGridService _sendGridService;

        public UsersActivities(
            IUserService userService,
            ISendGridService sendGridService)
        {
            _userService = userService;
            _sendGridService = sendGridService;
        }

        [FunctionName(Constants.FunctionNames.Activity.CreateUser)]
        public async Task<WorkflowStepResult> CreateUser([ActivityTrigger] (string OperatorEmail, UserModel User, List<Guid> GroupIds) input, ILogger logger)
        {
            try
            {
                var userId = await _userService.SaveAsync(input.OperatorEmail, input.User, input.GroupIds);
                return new WorkflowStepResult(
                    actionName: nameof(CreateUser),
                    outputId: userId);
            }
            catch (Exception ex)
            {
                return new WorkflowStepResult(
                    actionName: nameof(CreateUser),
                    outputId: Guid.Empty,
                    successfull: false, 
                    reason: $"Something went wrong creating user {input.User.UserName} - Error: {ex.Message}");
            }
        }

        [FunctionName(Constants.FunctionNames.Activity.UnassignGroupFromUser)]
        public async Task<WorkflowStepResult> UnassignGroupFromUser([ActivityTrigger] (Guid UserId, Guid GroupId) input, ILogger logger)
        {
            try
            {
                await _userService.UnassignGroupsFromUser(input.UserId, new List<Guid> { input.GroupId });
                return new WorkflowStepResult(
                    actionName: nameof(UnassignGroupFromUser),
                    outputId: input.GroupId);
            }
            catch (Exception ex)
            {
                return new WorkflowStepResult(
                    actionName: nameof(UnassignGroupFromUser),
                    outputId: input.GroupId, 
                    successfull: false,
                    reason: $"Something went wrong unassigning member {input.UserId} from group {input.GroupId} - Error: {ex.Message}");
            }
        }

        [FunctionName(Constants.FunctionNames.Activity.AskUserCreationApproval)]
        public async Task AskUserCreationApproval([ActivityTrigger] ProvisionNewUserSingleGroup input, ILogger logger)
        {
            try
            {
                await _sendGridService.SendEmail("noreply@coreview.com", "create user request", input.OperatorEmail, null, input.User.ToApprovalMailBody(input.CorrelationId));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
        }

        
    }
}
