using SagaToServerless.Services;
using System;
using System.Collections.Generic;
using SagaToServerless.Common;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SagaToServerless.Common.Models;
using Microsoft.Extensions.Logging;

namespace SagaToServerless.Durable.Activities.Miscellaneous
{
    public class NotificationActivities
    {
        private readonly ISendGridService _sendGridService;

        public NotificationActivities(
            ISendGridService sendGridService)
        {
            _sendGridService = sendGridService;
        }

        [FunctionName(Constants.FunctionNames.Activity.SendEmail)]
        public async Task SendEmail([ActivityTrigger] MailItemModel model, ILogger logger)
        {
            try
            {
                await _sendGridService.SendEmail(
                    from: string.Empty, 
                    subject: model.Subject, 
                    to: model.To, 
                    plainContent: model.PlainBody, 
                    htmlContent: model.HtmlBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
        }
    }
}
