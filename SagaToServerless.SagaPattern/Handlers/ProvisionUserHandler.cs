using MassTransit;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.SagaPattern.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Handlers
{
    public class ProvisionUserHandler :
        IConsumer<NewUserSingleGroupProvisioningSuccessfully>,
        IConsumer<NewUserSingleGroupProvisioningUnsuccessfully>,
        IConsumer<NewUserMultipleGroupsProvisioningCompleted>
    {
        public async Task Consume(ConsumeContext<NewUserSingleGroupProvisioningSuccessfully> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject: message.User.ToMailSubject(new List<Guid> { message.AssignToGroupId }),
                            htmlBody: message.User.ToMailBody(new List<Guid> { message.AssignedToGroupId })));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }

        public async Task Consume(ConsumeContext<NewUserSingleGroupProvisioningUnsuccessfully> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject: message.User.ToMailSubject(new List<Guid> { message.AssignToGroupId }),
                            htmlBody: message.User.ToMailBody(new List<Guid> { message.AssignedToGroupId }, message.Reason)));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }

        public async Task Consume(ConsumeContext<NewUserMultipleGroupsProvisioningCompleted> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject: message.User.ToMailSubject(message.AssignToGroupIds),
                            htmlBody: message.User.ToMailBody(message.AssignedToGroupIds, message.Reason)));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }
    }
}
