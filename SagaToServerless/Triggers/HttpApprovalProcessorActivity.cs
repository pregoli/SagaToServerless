using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common;

namespace SagaToServerless.Durable.Triggers
{
    public class HttpApprovalProcessorActivity
    {
        [FunctionName(Constants.FunctionNames.Trigger.TriggerHttpApprovalProcessor)]
        public static async Task<HttpResponseMessage> TriggerHttpApprovalProcessor(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "get", Route = "approval")] HttpRequestMessage req,
            [DurableClient] IDurableClient orchestrationClient, ILogger logger)
        {
            logger.LogInformation($"Received an Approval Respose");
            string instanceId = req.RequestUri.ParseQueryString().GetValues("instanceid")[0];
            string response = req.RequestUri.ParseQueryString().GetValues("response")[0];

            bool isApproved = false;

            var status = await orchestrationClient.GetStatusAsync(instanceId);
            logger.LogInformation($"Orchestration status: {status}");

            if (status != null && (status.RuntimeStatus == OrchestrationRuntimeStatus.Running || status.RuntimeStatus == OrchestrationRuntimeStatus.Pending))
            {
                if (response.ToLower() == "approved")
                    isApproved = true;

                await orchestrationClient.RaiseEventAsync(instanceId, "ReceiveApprovalResponse", isApproved);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("APPROVED!") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("OOPS!") };
            }
        }
    }
}
