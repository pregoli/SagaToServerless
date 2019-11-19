﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common.Models;
using SagaToServerless.Services;
using System;
using System.Collections.Generic;
using SagaToServerless.Common;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SagaToServerless.Durable.Dto;

namespace SagaToServerless.Durable.Activities.Groups
{
    public class GroupsActivities
    {
        private readonly IGroupService _groupService;
        public GroupsActivities(
            IGroupService groupService)
        {
            _groupService = groupService;
        }

        [FunctionName(Constants.FunctionNames.Activity.AssignUserToGroup)]
        public async Task<WorkflowStepResult> AssignUserToGroup([ActivityTrigger] AssignUserToGroupModel model, ILogger logger)
        {
            try
            {
                var groupId = await _groupService.AssignUserAsync(model.GroupId, model.UserId.ToString());
                return new WorkflowStepResult(nameof(AssignUserToGroup), groupId);
            }
            catch (Exception ex)
            {
                return new WorkflowStepResult(nameof(AssignUserToGroup), Guid.Empty, false, ex.Message);
            }
        }
    }
}
