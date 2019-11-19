using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using SagaToServerless.Common;
using SagaToServerless.Common.Commands;
using SagaToServerless.Common.Events;
using SagaToServerless.Common.Models;
using SagaToServerless.SagaPattern.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SagaToServerless.SagaPattern.Sagas
{
    public class ProvisionUserWithMultipleGroupsSaga : MassTransitStateMachine<ProvisionUserWithMultipleGroupsSagaState>
    {
        private const string SagaType = nameof(ProvisionUserWithMultipleGroupsSaga);
        public ProvisionUserWithMultipleGroupsSaga()
        {
            InstanceState(x => x.CurrentState);

            #region events registration

            Event(() => ProvisionNewUserMultipleGroups, x => x.CorrelateBy((s, m) => s.ParentCorrelationId == m.Message.CorrelationId && s.SagaType == SagaType).SelectId(ctx => Guid.NewGuid()));
            Event(() => CreateUser, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserCreatedSuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserCreatedUnsuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => AssignUserToGroups, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => UserAssignedToGroupsCompleted, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));

            #endregion

            Initially(
                When(ProvisionNewUserMultipleGroups)
                    .Then(x =>
                    {
                        x.Instance.ParentCorrelationId = x.Data.CorrelationId;
                        x.Instance.StartDate = DateTime.UtcNow;
                        x.Instance.SagaType = SagaType;
                        x.Instance.OperatorEmail = x.Data.OperatorEmail;
                        x.Instance.User = x.Data.User;
                        x.Instance.AssignToGroupIds = x.Data.GroupIds;
                    })
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.UsersHandler),
                        x => new CreateUser(x.Instance.CorrelationId, x.Instance.User, x.Instance.OperatorEmail))
                    .TransitionTo(CreatingUser));

            During(CreatingUser,
                When(UserCreatedSuccessfully)
                    .Then(x => x.Instance.UserId = x.Data.UserId)
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.GroupsHandler),
                       x => new AssignUserToGroups(x.Instance.CorrelationId, x.Instance.AssignToGroupIds, x.Instance.UserId ))
                    .TransitionTo(AssigningUserToGroups),
                When(UserCreatedUnsuccessfully)
                    .Then(x =>
                    {
                        x.Instance.ErrorMessage = x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .TransitionTo(Failed));

            During(AssigningUserToGroups,
                When(UserAssignedToGroupsCompleted)
                    .Then(x => x.Instance.AssignedToGroupIds.AddRange(x.Data.AssignedGroupIds.Where(outputId => outputId != Guid.Empty)))
                    .If(x => !x.Data.Successfull, binder => binder
                        .Then(x =>
                        {
                            x.Instance.ErrorMessage = x.Data.Reason;
                            x.Instance.EndDate = DateTime.UtcNow;
                        })
                        .Publish(x => new NewUserMultipleGroupsProvisioningCompleted(
                            x.Instance.CorrelationId,
                            x.Instance.AssignToGroupIds,
                            x.Instance.AssignedToGroupIds,
                            x.Instance.User,
                            x.Instance.OperatorEmail,
                            x.Instance.ErrorMessage))
                        .TransitionTo(PartiallyCompleted))
                    .If(x => x.Data.Successfull, binder => binder
                        .Publish(x => new NewUserMultipleGroupsProvisioningCompleted(
                            x.Instance.CorrelationId,
                            x.Instance.AssignToGroupIds,
                            x.Instance.AssignedToGroupIds,
                            x.Instance.User,
                            x.Instance.OperatorEmail,
                            x.Instance.ErrorMessage))
                        .Then(x => x.Instance.EndDate = DateTime.UtcNow)
                        .Finalize()));
        }

        #region Events

        public Event<ProvisionNewUserMultipleGroups> ProvisionNewUserMultipleGroups { get; private set; }
        public Event<CreateUser> CreateUser { get; private set; }
        public Event<UserCreatedSuccessfully> UserCreatedSuccessfully { get; private set; }
        public Event<UserCreatedUnsuccessfully> UserCreatedUnsuccessfully { get; private set; }
        public Event<AssignUserToGroups> AssignUserToGroups { get; private set; }
        public Event<UserAssignedToGroupsCompleted> UserAssignedToGroupsCompleted { get; private set; }

        #endregion

        #region State

        public State CreatingUser { get; private set; }
        public State AssigningUserToGroups { get; private set; }
        public State PartiallyCompleted { get; private set; }
        public State Failed { get; private set; }

        #endregion
    }

    public class ProvisionUserWithMultipleGroupsSagaState : SagaStateMachineInstance, IVersionedSaga
    {
        public ProvisionUserWithMultipleGroupsSagaState()
        {
            AssignedToGroupIds = new List<Guid>();
            AssignToGroupIds = new List<Guid>();
        }

        [BsonId]
        public Guid CorrelationId { get; set; }
        public Guid ParentCorrelationId { get; set; }
        public int Version { get; set; }
        public string SagaType { get; set; }
        public string CurrentState { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public UserModel User { get; set; }
        public string OperatorEmail { get; set; }
        public Guid UserId { get; set; }
        public List<Guid> AssignToGroupIds { get; set; }
        public List<Guid> AssignedToGroupIds { get; set; }
        public string ErrorMessage { get; set; }
    }
}
