using MassTransit;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Handlers
{
    public class UsersHandler : 
        IConsumer<CreateUser>,
        IConsumer<UnassignGroupsFromUser>
    {
        private readonly IUserService _userService;

        public UsersHandler(
            IUserService userService)
        {
            _userService = userService;
        }

        public async Task Consume(ConsumeContext<CreateUser> context)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var message = context.Message;

            try
            {
                var userId = await _userService.SaveAsync(message.CreatedBy, message.User, message.GroupIds);
                await context.Publish(new UserCreatedSuccessfully(
                    correlationId: message.CorrelationId,
                    userId: userId));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserCreatedUnsuccessfully(
                    correlationId: message.CorrelationId,
                    reason: $"Something went wrong creating user {message.User.UserName} - Error: {ex.Message}"));
            }
        }

        public async Task Consume(ConsumeContext<UnassignGroupsFromUser> context)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var message = context.Message;

            try
            {
                await _userService.UnassignGroupsFromUser(message.UserId, message.GroupIds);
                await context.Publish(new GroupsUnassignedFromUserSuccessfully(message.CorrelationId, message.GroupIds));
            }
            catch (Exception ex)
            {
                await context.Publish(new GroupsUnassignedFromUserUnsuccessfully(message.CorrelationId, message.GroupIds, ex.Message));
            }
        }
    }
}
