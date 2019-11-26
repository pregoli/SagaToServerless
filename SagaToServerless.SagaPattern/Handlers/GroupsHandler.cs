using MassTransit;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Handlers
{
    public class GroupsHandler : 
        IConsumer<AssignUserToGroup>,
        IConsumer<AssignUserToGroups>
    {
        private readonly IGroupService _groupService;
        public GroupsHandler(
            IGroupService groupService)
        {
            _groupService = groupService;
        }

        public async Task Consume(ConsumeContext<AssignUserToGroup> context)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var message = context.Message;

            try
            {
                var groupId = await _groupService.AssignUserAsync(message.GroupId, message.UserId.ToString());
                await context.Publish(new UserAssignedToGroupSuccessfully(message.CorrelationId, groupId));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserAssignedToGroupUnsuccessfully(
                    correlationId: message.CorrelationId, 
                    groupId: message.GroupId, 
                    reason: $"Something went wrong adding the user {message.UserId} to group {message.GroupId} members - Error: {ex.Message}"));
            }
        }

        public async Task Consume(ConsumeContext<AssignUserToGroups> context)
        {
            var assignedGroupIds = new List<Guid>();

            var message = context.Message;

            try
            {
                var tasks = new List<Task<Guid>>();
                for (int i = 0; i < message.GroupIds.Count; i++)
                    tasks.Add(_groupService.AssignUserAsync(message.GroupIds[i], message.UserId.ToString()));

                assignedGroupIds = (await Task.WhenAll(tasks)).ToList();

                await context.Publish(new UserAssignedToGroupsCompleted(
                    correlationId: message.CorrelationId,
                    assignedGroupIds: assignedGroupIds));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserAssignedToGroupsCompleted(
                    correlationId: message.CorrelationId,
                    assignedGroupIds: assignedGroupIds, 
                    successfull: false,
                    reason: $"Something went wrong adding the user {message.UserId} to groups {string.Join(',', message.GroupIds)} members - Error: {ex.Message}"));
            }
        }
    }
}
