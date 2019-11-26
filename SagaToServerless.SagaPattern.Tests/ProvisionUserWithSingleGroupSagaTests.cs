using MassTransit;
using MassTransit.Saga;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.SagaPattern.Sagas;
using System;
using System.Threading.Tasks;
using MassTransit.Testing;
using NUnit.Framework;
using System.Collections.Generic;

namespace SagaToServerless.SagaPattern.Tests
{
    [TestFixture]
    public class ProvisionUserWithSingleGroupSagaTests : ProvisionUserWithSingleGroupSagaTestBase
    {
        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            base.ConfigureInMemoryReceiveEndpoint(configurator);

            _sagaRepository = new InMemorySagaRepository<ProvisionUserWithSingleGroupSagaState>();
            _saga = new ProvisionUserWithSingleGroupSaga();
            configurator.StateMachineSaga(_saga, _sagaRepository);
        }

        public ProvisionUserWithSingleGroupSagaTests() : base(true)
        {

        }

        #region All Good

        [Test]
        public async Task Publishing_A_CreateUser_Command_During_Initial_State_The_Saga_Will_Move_On_CreatingUser_State()
        {
            Task<ConsumeContext<CreateUser>> handler = null;
            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e => handler = Handled<CreateUser>(e));

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);
            await handler;

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.CreatingUser);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        [Test]
        public async Task Receiving_A_UserCreatedSuccessfully_During_CreatingUser_State_The_Saga_Will_Move_On_AssigningUserToGroup_State()
        {
            Task<ConsumeContext<AssignUserToGroup>> groupsHandler = null;
            Host.ConnectReceiveEndpoint(GroupsHandlerEndpoint, e => groupsHandler = Handled<AssignUserToGroup>(e));

            var newUserId = Guid.NewGuid();

            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedSuccessfully(ctx.Message.CorrelationId, newUserId));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);
            await groupsHandler;

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.AssigningUserToGroup);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        [Test]
        public async Task Receiving_A_UserAssignedToGroupSuccessfully_During_AssigningUserToGroup_State_The_Saga_Will_Move_On_Final_State()
        {
            var newUserId = Guid.NewGuid();

            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedSuccessfully(ctx.Message.CorrelationId, newUserId));
                        }));
            });
            Host.ConnectReceiveEndpoint(GroupsHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<AssignUserToGroup>(
                        ctx =>
                        {
                            return ctx.Publish(new UserAssignedToGroupSuccessfully(ctx.Message.CorrelationId, ctx.Message.GroupId));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.Final);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        #endregion

        [Test]
        public async Task Receiving_A_UserCreatedUnsuccessfully_During_CreatingUser_State_The_Saga_Will_Move_On_Failed_State()
        {
            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedUnsuccessfully(ctx.Message.CorrelationId, "User creation failed!"));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.Failed);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        [Test]
        public async Task Receiving_A_UserAssignedToGroupUnsuccessfully_During_AssigningUserToGroup_State_The_Saga_Will_Move_On_UnassigningGroupFromUser_State()
        {
            var newUserId = Guid.NewGuid();

            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedSuccessfully(ctx.Message.CorrelationId, newUserId));
                        }));
            });
            Host.ConnectReceiveEndpoint(GroupsHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<AssignUserToGroup>(
                        ctx =>
                        {
                            return ctx.Publish(new UserAssignedToGroupUnsuccessfully(ctx.Message.CorrelationId, Guid.Empty, "Group assignment failed!"));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.UnassigningGroupFromUser);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        [Test]
        public async Task Receiving_A_GroupsUnassignedFromUserSuccessfully_During_UnassigningGroupFromUser_State_The_Saga_Will_Move_On_PartiallyCompleted_State()
        {
            var newUserId = Guid.NewGuid();

            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedSuccessfully(ctx.Message.CorrelationId, newUserId));
                        }));
                Handler(e,
                    new MessageHandler<UnassignGroupsFromUser>(
                        ctx =>
                        {
                            return ctx.Publish(new GroupsUnassignedFromUserSuccessfully(ctx.Message.CorrelationId, new List<Guid>()));
                        }));
            });
            Host.ConnectReceiveEndpoint(GroupsHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<AssignUserToGroup>(
                        ctx =>
                        {
                            return ctx.Publish(new UserAssignedToGroupUnsuccessfully(ctx.Message.CorrelationId, Guid.Empty, "Group assignment failed!"));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.PartiallyCompleted);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }

        [Test]
        public async Task Receiving_A_GroupsUnassignedFromUserUnsuccessfully_During_UnassigningGroupFromUser_State_The_Saga_Will_Move_On_PartiallyCompleted_State()
        {
            var newUserId = Guid.NewGuid();

            Host.ConnectReceiveEndpoint(UsersHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<CreateUser>(
                        ctx =>
                        {
                            return ctx.Publish(new UserCreatedSuccessfully(ctx.Message.CorrelationId, newUserId));
                        }));
                Handler(e,
                    new MessageHandler<UnassignGroupsFromUser>(
                        ctx =>
                        {
                            return ctx.Publish(new GroupsUnassignedFromUserUnsuccessfully(ctx.Message.CorrelationId, new List<Guid>(), "boom"));
                        }));
            });
            Host.ConnectReceiveEndpoint(GroupsHandlerEndpoint, e =>
            {
                Handler(e,
                    new MessageHandler<AssignUserToGroup>(
                        ctx =>
                        {
                            return ctx.Publish(new UserAssignedToGroupUnsuccessfully(ctx.Message.CorrelationId, Guid.Empty, "Group assignment failed!"));
                        }));
            });

            var command = CreateProvisionNewUserSingleGroupCommand();
            await InputQueueSendEndpoint.Send(command);

            var sagaId = await ShouldContainSaga(command.CorrelationId, _saga.PartiallyCompleted);
            var instance = _sagaRepository[sagaId.Value].Instance;

            Assert.AreEqual(nameof(ProvisionUserWithSingleGroupSaga), instance.SagaType);
            Assert.AreEqual(command.CorrelationId, instance.ParentCorrelationId);
            Assert.AreEqual(command.User.UserName, instance.User.UserName);
        }
    }
}
