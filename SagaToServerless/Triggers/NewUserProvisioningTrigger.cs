using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;

namespace SagaToServerless.Durable.Triggers
{
    public class NewUserProvisioningTrigger
    {
        #region Chaining pattern

        [FunctionName(Constants.FunctionNames.Trigger.TriggerUserProvisioningChainingWorkflow)]
        public async Task TriggerUserProvisioningChainingWorkflow(
         [RabbitMQTrigger("chainingqueue", ConnectionStringSetting = "rabbitMQ")] ProvisionNewUserSingleGroup command,
        [DurableClient] IDurableClient starter,
        ILogger logger)
        {
            await starter.StartNewAsync(Constants.FunctionNames.Orchestrator.StartProvisionUserWithSingleGroupOrchestrator, command.CorrelationId.ToString(), command);
        }

        #endregion

        #region FanIn-FanOut pattern

        [FunctionName(Constants.FunctionNames.Trigger.TriggerUserProvisioningFanInFanOutWorkflow)]
        public async Task TriggerUserProvisioningFanInFanOutWorkflow(
        [RabbitMQTrigger("faninfanoutqueue", ConnectionStringSetting = "rabbitMQ")] ProvisionNewUserMultipleGroups command,
        [DurableClient] IDurableClient starter,
        ILogger logger)
        {
            await starter.StartNewAsync(Constants.FunctionNames.Orchestrator.StartProvisionUserWithMultipleGroupsOrchestrator, command.CorrelationId.ToString(), command);
        }

        #endregion

        #region Human interaction / approval pattern

        [FunctionName(Constants.FunctionNames.Trigger.TriggerUserProvisioningApprovalWorkflow)]
        public async Task TriggerUserProvisioningApprovalWorkflow(
        [RabbitMQTrigger("approvalqueue", ConnectionStringSetting = "rabbitMQ")] ProvisionNewUserSingleGroup command,
        [DurableClient] IDurableClient starter,
        ILogger logger)
        {
            await starter.StartNewAsync(Constants.FunctionNames.Orchestrator.ExecuteUserProvisioninApprovalgWorkflow, command.CorrelationId.ToString(), command);
        }

        #endregion
    }
}
