using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Common.Models
{
    public class MailItemModel
    {
        public MailItemModel()
        {

        }

        public MailItemModel(
            string from,
            string to,
            string subject,
            string plainBody = "",
            string htmlBody = "")
        {
            From = from;
            To = to;
            Subject = subject;
            PlainBody = plainBody;
            HtmlBody = htmlBody;
        }

        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string PlainBody { get; set; }
        public string HtmlBody { get; set; }
    }
}
