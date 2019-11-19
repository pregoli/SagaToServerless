using MassTransit;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.Services;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Handlers
{
    public class NotificationsHandler : IConsumer<NotifyByEmail>
    {
        private readonly ISendGridService _sendGridService;

        public NotificationsHandler(
            ISendGridService sendGridService)
        {
            _sendGridService = sendGridService;
        }

        public async Task Consume(ConsumeContext<NotifyByEmail> context)
        {
            var message = context.Message;

            try
            {
                await _sendGridService.SendEmail(
                    string.Empty, 
                    message.Subject, 
                    message.To, 
                    message.PlainBody, 
                    message.HtmlBody);

                await context.Publish(new NotificationSentSuccessfully(message.CorrelationId));
            }
            catch (Exception ex)
            {
                await context.Publish(new NotificationSentUnsuccessfully(message.CorrelationId, ex.Message));
            }
        }
    }
}
