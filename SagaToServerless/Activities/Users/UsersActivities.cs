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
        public async Task<WorkflowStepResult> CreateUser([ActivityTrigger] (string OperatorEmail, UserModel User) request, ILogger logger)
        {
            try
            {
                var newUserId = await _userService.SaveAsync(request.OperatorEmail, request.User);
                return new WorkflowStepResult(nameof(CreateUser), newUserId);
            }
            catch (Exception ex)
            {
                return new WorkflowStepResult(nameof(CreateUser), Guid.Empty, false, ex.Message);
            }
        }

        [FunctionName(Constants.FunctionNames.Activity.AskUserCreationApproval)]
        public async Task AskUserCreationApproval([ActivityTrigger] ProvisionNewUserSingleGroup model, ILogger logger)
        {
            try
            {
                await _sendGridService.SendEmail("noreply@coreview.com", "create user request", model.OperatorEmail, $"Create user {model.User.FirstName} {model.User.LastName}", ApprovalMailBody(model.CorrelationId));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
        }

        private string ApprovalMailBody(Guid instanceId)
        {
            return
                $@"<a href='http://localhost:7071/api/approval?instanceid={instanceId}&response=approved'>Approve</a>
                <br>
                <a href='http://localhost:7071/api/approval?instanceid={instanceId}&response=rejected'>Reject</a>";
        }
    }
}
