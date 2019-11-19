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
    public class UsersHandler : IConsumer<CreateUser>
    {
        private readonly IUserService _userService;

        public UsersHandler(
            IUserService userService)
        {
            _userService = userService;
        }

        public async Task Consume(ConsumeContext<CreateUser> context)
        {
            var message = context.Message;

            try
            {
                var newUserId = await _userService.SaveAsync(message.CreatedBy, message.User);
                await context.Publish(new UserCreatedSuccessfully(message.CorrelationId, newUserId));
            }
            catch (Exception ex)
            {
                await context.Publish(new UserCreatedUnsuccessfully(message.CorrelationId, ex.Message));
            }
        }
    }
}
