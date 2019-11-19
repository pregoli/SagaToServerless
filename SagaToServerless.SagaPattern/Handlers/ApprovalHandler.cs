using MassTransit;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.SagaPattern.Handlers
{
    public class ApprovalHandler :
        IConsumer<AskApproval>,
        IConsumer<SendApproval>
    {
        public async Task Consume(ConsumeContext<SendApproval> context)
        {
            var message = context.Message;
            await context.Publish(new ApprovalReceived(message.CorrelationId, message.Approved));
        }

        public async Task Consume(ConsumeContext<AskApproval> context)
        {
            var message = context.Message;
            await context.Publish(new NotifyByEmail(
                correlationId: message.CorrelationId,
                from: string.Empty,
                to: message.OperatorEmail,
                subject: $"Provisioning User {message.User.FirstName} {message.User.LastName} with Group {message.GroupId}",
                htmlBody: ApprovalMailBody(message.CorrelationId)));
        }

        private string ApprovalMailBody(Guid correlationId)
        {
            return
                $@"<a style='color: green;' href='http://localhost:7071/api/provisionuser/approval?correlationId={correlationId}&response=approved'>Approve</a>
                <br>
                <a style='color: red;' href='http://localhost:7071/api/provisionuser/approval?correlationId={correlationId}&response=rejected'>Reject</a>";
        }
    }
}
