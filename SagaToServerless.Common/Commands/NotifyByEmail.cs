using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Commands
{
    public class NotifyByEmail : ICommand
    {
        public NotifyByEmail(
            Guid correlationId,
            string from,
            string to,
            string subject,
            string plainBody = "",
            string htmlBody = "")
        {
            CorrelationId = correlationId;
            From = from;
            To = to;
            Subject = subject;
            PlainBody = plainBody;
            HtmlBody = htmlBody;
        }

        public Guid CorrelationId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string PlainBody { get; set; }
        public string HtmlBody { get; set; }
    }
}
