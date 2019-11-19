using Automatonymous;
using MassTransit;
using MassTransit.Saga;
using MassTransit.TestFramework;
using MassTransit.Testing;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;
using SagaToServerless.SagaPattern.Sagas;
using System;
using System.Threading.Tasks;
using MassTransit.Util;
using System.Configuration;

namespace SagaToServerless.SagaPattern.Tests
{
    public abstract class ProvisionUserWithSingleGroupSagaTestBase : InMemoryTestFixture
    {
        protected InMemorySagaRepository<ProvisionUserWithSingleGroupSagaState> _sagaRepository;
        protected ProvisionUserWithSingleGroupSaga _saga;
        protected Guid _correlationId;
        protected IInMemoryReceiveEndpointConfigurator _configurator;

        protected string UsersHandlerEndpoint = Constants.SagaPattern.QueueNames.UsersHandlerQueue;
        protected string GroupsHandlerEndpoint = Constants.SagaPattern.QueueNames.GroupsHandlerQueue;

        public ProvisionUserWithSingleGroupSagaTestBase(bool busPerTest = false) : base(busPerTest)
        {
        }

        protected Task<Guid?> ShouldContainSaga(Guid correlatedId, State currentState)
        {
            var result = _sagaRepository.ShouldContainSaga(
                x => x.ParentCorrelationId == correlatedId &&
                     MassTransitSagaHelper.GetCurrentState(x, _saga).Result == currentState,
                TestTimeout);

            return result;
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            base.ConfigureInMemoryReceiveEndpoint(configurator);

            _sagaRepository = new InMemorySagaRepository<ProvisionUserWithSingleGroupSagaState>();
            _saga = new ProvisionUserWithSingleGroupSaga();

            configurator.StateMachineSaga(_saga, _sagaRepository);
        }

        protected ProvisionNewUserSingleGroup CreateProvisionNewUserSingleGroupCommand()
        {
            return new ProvisionNewUserSingleGroup
            {
                CorrelationId = Guid.NewGuid(),
                GroupId = Guid.NewGuid(),
                OperatorEmail = "polo.regoli@coreview.com",
                User = new Common.Models.UserModel
                {
                    FirstName = "testName",
                    LastName = "testLastName",
                    UserName = "testName.lastName@gmal.com"
                }
            };
        }
    }
}
