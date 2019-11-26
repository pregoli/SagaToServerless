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
        IConsumer<NewUserSingleGroupProvisioningCompleted>,
        IConsumer<NewUserProvisioningWithApprovalCompleted>,
        IConsumer<NewUserMultipleGroupsProvisioningCompleted>,
        IConsumer<NewUserSingleGroupCreationRejected>
    {
        public async Task Consume(ConsumeContext<NewUserSingleGroupProvisioningCompleted> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject:  message.User.ToMailSubject(new List<Guid> { message.AssignToGroupId }),
                            htmlBody: message.User.ToMailBody(message.UserCreated, new List <Guid> { message.AssignedToGroupId }, message.Reason)));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }

        public async Task Consume(ConsumeContext<NewUserProvisioningWithApprovalCompleted> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject: message.User.ToMailSubject(new List<Guid> { message.AssignToGroupId }),
                            htmlBody: !message.Approved ? message.User.ToApprovalRejectedMailBody(message.Reason) : message.User.ToMailBody(message.UserCreated, new List<Guid> { message.AssignedToGroupId }, message.Reason)));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }

        public async Task Consume(ConsumeContext<NewUserSingleGroupCreationRejected> context)
        {
            try
            {
                var message = context.Message;
                await context.Publish(new NotifyByEmail(
                            correlationId: message.CorrelationId,
                            from: string.Empty,
                            to: message.OperatorEmail,
                            subject: $"Provision user {message.User.UserName}",
                            htmlBody: $"<string style='color:red'>REJECTED</string>"));
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
                            htmlBody: message.User.ToMailBody(message.UserCreated, message.AssignedToGroupIds, message.Reason)));
            }
            catch (Exception ex)
            {
                //log something maybe...
            }
        }
    }
}
