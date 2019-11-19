using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public class SendGridService : ISendGridService
    {
        private readonly SendGridClient _client;
        public SendGridService(string apiKey)
        {
            _client = new SendGridClient(apiKey);
        }

        public async Task SendEmail(
            string from,
            string subject,
            string to,
            string plainContent,
            string htmlContent)
        {
            var fromEmailAddress = new EmailAddress("noreply@coreview.com");
            var toEmailAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(fromEmailAddress, toEmailAddress, subject, plainContent, htmlContent);
            try
            {
                await _client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
