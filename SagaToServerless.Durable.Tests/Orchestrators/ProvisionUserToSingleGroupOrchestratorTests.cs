using Microsoft.VisualStudio.TestTools.UnitTesting;
using SagaToServerless.Common.Models;
using SagaToServerless.Durable.Dto;
using SagaToServerless.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SagaToServerless.Common.Commands;
using SagaToServerless.Durable.Orchestrators;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace SagaToServerless.Durable.Tests.Orchestrators
{
    [TestClass]
    public class ProvisionUserToSingleGroupOrchestratorTests
    {
        private readonly ILogger _logger = NullLoggerFactory.Instance.CreateLogger("test");

        #region mocked create user request/response
        private (string OperatorEmail, UserModel User) MockedProvisionedUserModel => (
                OperatorEmail: "paolo.regoli@coreview.com",
                User: new UserModel
                {
                    FirstName = "carlo",
                    LastName = "conti",
                    UserName = "carlo.conti@gmail.com"
                }
            );

        private Guid MockedNewUserId = Guid.NewGuid();
        private WorkflowStepResult MockedCreateUserStepResult => new WorkflowStepResult(Constants.FunctionNames.Activity.CreateUser, MockedNewUserId, true);
        #endregion

        #region mocked create group request/response
        private Guid MockedGroupId = Guid.NewGuid();
        private WorkflowStepResult MockedAssignUserToGroupStepResult => new WorkflowStepResult(Constants.FunctionNames.Activity.AssignUserToGroup, MockedGroupId, true);
        #endregion

        [TestMethod]
        public async Task Run_Chaining_Workflow()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(x => x.GetInput<ProvisionNewUserSingleGroup>()).Returns(new ProvisionNewUserSingleGroup
            {
                CorrelationId = Guid.NewGuid(),
                GroupId = MockedGroupId,
                User = MockedProvisionedUserModel.User,
                OperatorEmail = MockedProvisionedUserModel.OperatorEmail
            });

            contextMock
                .Setup(context => context.CallActivityWithRetryAsync<WorkflowStepResult>(
                    Constants.FunctionNames.Activity.CreateUser,
                    It.IsAny<RetryOptions>(),
                    It.IsAny<(string OperatorEmail, UserModel user)>()))
                .Returns(Task.FromResult(MockedCreateUserStepResult));

            contextMock
                .Setup(context => context.CallActivityWithRetryAsync<WorkflowStepResult>(
                    Constants.FunctionNames.Activity.AssignUserToGroup,
                    It.IsAny<RetryOptions>(),
                    It.IsAny<AssignUserToGroupModel>()))
                .Returns(Task.FromResult(MockedAssignUserToGroupStepResult));

            var orchestrator = new ProvisionUserWithSingleGroupOrchestrator();

            var result = await orchestrator.StartProvisionUserWithSingleGroupOrchestrator(
                contextMock.Object,
                _logger);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(MockedNewUserId, result[0].OutputId);
            Assert.AreEqual(MockedGroupId, result[1].OutputId);
        }
    }
}
