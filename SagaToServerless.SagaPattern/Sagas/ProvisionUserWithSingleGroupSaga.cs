﻿using Automatonymous;
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
using System.Linq;
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
            Event(() => GroupsUnassignedFromUserSuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => GroupsUnassignedFromUserUnsuccessfully, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));

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
                        x => new CreateUser(
                            x.Instance.CorrelationId, 
                            x.Instance.User, 
                            new List<Guid> { x.Instance.GroupId }, 
                            x.Instance.OperatorEmail))
                    .TransitionTo(CreatingUser));

            During(CreatingUser,
                When(UserCreatedSuccessfully)
                    .Then(x => x.Instance.NewUserId = x.Data.UserId)
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.GroupsHandler),
                       x => new AssignUserToGroup(
                           x.Instance.CorrelationId, 
                           x.Instance.GroupId, 
                           x.Instance.NewUserId))
                    .TransitionTo(AssigningUserToGroup),
                When(UserCreatedUnsuccessfully)
                    .Then(x => 
                    { 
                        x.Instance.ErrorMessage = x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .Publish(x => new NewUserSingleGroupProvisioningCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        false,
                        x.Instance.ErrorMessage))
                    .TransitionTo(Failed));

            During(AssigningUserToGroup,
                When(UserAssignedToGroupSuccessfully)
                    .Then(x =>
                    {
                        x.Instance.AssignedGroupId = x.Data.AssignedGroupId;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .Publish(x => new NewUserSingleGroupProvisioningCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        true))
                    .Finalize(),
                When(UserAssignedToGroupUnsuccessfully)
                    .Then(x => x.Instance.ErrorMessage += x.Data.Reason )
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.UsersHandler),
                        x => new UnassignGroupsFromUser(x.Instance.CorrelationId, x.Instance.NewUserId, new List<Guid> { x.Instance.GroupId }))
                    .TransitionTo(UnassigningGroupFromUser));

            During(UnassigningGroupFromUser,
                When(GroupsUnassignedFromUserSuccessfully)
                    .Then(x => x.Instance.EndDate = DateTime.UtcNow)
                    .Publish(x => new NewUserSingleGroupProvisioningCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        true,
                        x.Instance.ErrorMessage))
                    .TransitionTo(PartiallyCompleted),
                When(GroupsUnassignedFromUserUnsuccessfully)
                    .Then(x =>
                    {
                        x.Instance.ErrorMessage += x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .Publish(x => new NewUserSingleGroupProvisioningCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        true,
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
        public Event<GroupsUnassignedFromUserSuccessfully> GroupsUnassignedFromUserSuccessfully { get; private set; }
        public Event<GroupsUnassignedFromUserUnsuccessfully> GroupsUnassignedFromUserUnsuccessfully { get; private set; }

        #endregion

        #region State

        public State CreatingUser { get; private set; }
        public State AssigningUserToGroup { get; private set; }
        public State UnassigningGroupFromUser { get; private set; }
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
        [BsonRepresentation(BsonType.String)]
        public Guid NewUserId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid GroupId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid AssignedGroupId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
