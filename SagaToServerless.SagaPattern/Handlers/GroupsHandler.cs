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
            var message = context.Message;

            try
            {
                var groupId = await _groupService.AssignUserAsync(message.GroupId, message.UserId.ToString());
                await context.Publish(new UserAssignedToGroupSuccessfully(message.CorrelationId, groupId));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserAssignedToGroupUnsuccessfully(message.CorrelationId, message.GroupId, ex.Message));
            }
        }

        public async Task Consume(ConsumeContext<AssignUserToGroups> context)
        {
            var result = new List<Guid>();

            var message = context.Message;

            try
            {
                var tasks = new List<Task<Guid>>();
                for (int i = 0; i < message.GroupIds.Count; i++)
                    tasks.Add(_groupService.AssignUserAsync(message.GroupIds[i], message.UserId.ToString()));

                result = (await Task.WhenAll(tasks)).ToList();

                await context.Publish(new UserAssignedToGroupsCompleted(message.CorrelationId, result));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserAssignedToGroupsCompleted(message.CorrelationId, result, false, ex.Message));
            }
        }
    }
}
