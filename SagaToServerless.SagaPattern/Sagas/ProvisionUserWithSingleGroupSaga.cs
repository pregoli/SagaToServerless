using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.Common.Models;
using SagaToServerless.SagaPattern.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace SagaToServerless.SagaPattern.Sagas
{
    public class ProvisionUserWithSingleGroupSaga : MassTransitStateMachine<ProvisionUserWithSingleGroupSagaState>
    {
        private const string SagaType = nameof(ProvisionUserWithSingleGroupSaga);

        public ProvisionUserWithSingleGroupSaga()
        {
            InstanceState(x => x.CurrentState);

            #region events registration

            Event(() => ProvisionNewUserSingleGroup, x => x.CorrelateBy((s, m) => s.ParentCorrelationId == m.Message.CorrelationId && s.SagaType == SagaType).SelectId(ctx => Guid.NewGuid()));
            Event(() => CreateUser, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserCreatedSuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserCreatedUnsuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => AssignUserToGroup, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserAssignedToGroupSuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserAssignedToGroupUnsuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            //Event(() => NotifyByEmail, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));

            #endregion

            Initially(
                When(ProvisionNewUserSingleGroup)
                    .Then(x =>
                    {
                        x.Instance.ParentCorrelationId = x.Data.CorrelationId;
                        x.Instance.StartDate = DateTime.UtcNow;
                        x.Instance.SagaType = SagaType;
                        x.Instance.OperatorEmail = x.Data.OperatorEmail;
                        x.Instance.User = x.Data.User;
                        x.Instance.GroupId = x.Data.GroupId;
                    })
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.UsersHandler),
                        x => new CreateUser(x.Instance.CorrelationId, x.Instance.User, x.Instance.OperatorEmail))
                    .TransitionTo(CreatingUser));

            During(CreatingUser,
                When(UserCreatedSuccessfully)
                    .Then(x => x.Instance.NewUserId = x.Data.UserId)
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.GroupsHandler),
                       x => new AssignUserToGroup(x.Instance.CorrelationId, x.Instance.GroupId, x.Instance.NewUserId))
                    .TransitionTo(AssigningUserToGroup),
                When(UserCreatedUnsuccessfully)
                    .Then(x => 
                    { 
                        x.Instance.ErrorMessage = x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .TransitionTo(Failed));

            During(AssigningUserToGroup,
                When(UserAssignedToGroupSuccessfully)
                    .Then(x => x.Instance.EndDate = DateTime.UtcNow)
                    .Publish(x => new NewUserSingleGroupProvisioningSuccessfully(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Data.OutputId,
                        x.Instance.User,
                        x.Instance.OperatorEmail))
                    .Finalize(),
                When(UserAssignedToGroupUnsuccessfully)
                    .Then(x =>
                    {
                        x.Instance.ErrorMessage += x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .Publish(x => new NewUserSingleGroupProvisioningUnsuccessfully(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Data.OutputId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        x.Instance.ErrorMessage))
                    .TransitionTo(PartiallyCompleted));
        }

        #region Events

        public Event<ProvisionNewUserSingleGroup> ProvisionNewUserSingleGroup { get; private set; }
        public Event<CreateUser> CreateUser { get; private set; }
        public Event<UserCreatedSuccessfully> UserCreatedSuccessfully { get; private set; }
        public Event<UserCreatedUnsuccessfully> UserCreatedUnsuccessfully { get; private set; }
        public Event<AssignUserToGroup> AssignUserToGroup { get; private set; }
        public Event<UserAssignedToGroupSuccessfully> UserAssignedToGroupSuccessfully { get; private set; }
        public Event<UserAssignedToGroupUnsuccessfully> UserAssignedToGroupUnsuccessfully { get; private set; }
        //public Event<NotifyByEmail> NotifyByEmail { get; private set; }

        #endregion

        #region State

        public State CreatingUser { get; private set; }
        public State AssigningUserToGroup { get; private set; }
        public State PartiallyCompleted { get; private set; }
        public State Failed { get; private set; }

        #endregion
    }

    public class ProvisionUserWithSingleGroupSagaState : SagaStateMachineInstance, IVersionedSaga
    {
        [BsonId(IdGenerator = typeof(GuidGenerator)), BsonRepresentation(BsonType.String)]
        public Guid CorrelationId { get; set; }
        public Guid ParentCorrelationId { get; set; }
        public int Version { get; set; }
        public string SagaType { get; set; }
        public string CurrentState { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
        public Guid NewUserId { get; set; }
        public Guid GroupId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
