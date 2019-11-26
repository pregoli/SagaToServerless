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
using System.Linq;
using System.Text;

namespace SagaToServerless.SagaPattern.Sagas
{
    public class ProvisionUserWithApprovalSaga : MassTransitStateMachine<ProvisionUserWithApprovalSagaState>
    {
        private const string SagaType = nameof(ProvisionUserWithApprovalSaga);

        public ProvisionUserWithApprovalSaga()
        {
            InstanceState(x => x.CurrentState);

            #region events registration

            Event(() => ProvisionNewUserSingleGroupWithApproval, x => x.CorrelateBy((s, m) => s.ParentCorrelationId == m.Message.CorrelationId && s.SagaType == SagaType).SelectId(ctx => Guid.NewGuid()));
            Event(() => AskApproval, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
            Event(() => ApprovalReceived, x => x.CorrelateBy((s, m) => s.CorrelationId == m.Message.CorrelationId));
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
                When(ProvisionNewUserSingleGroupWithApproval)
                    .Then(x =>
                    {
                        x.Instance.ParentCorrelationId = x.Data.CorrelationId;
                        x.Instance.StartDate = DateTime.UtcNow;
                        x.Instance.SagaType = SagaType;
                        x.Instance.OperatorEmail = x.Data.OperatorEmail;
                        x.Instance.User = x.Data.User;
                        x.Instance.GroupId = x.Data.GroupId;
                    })
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.ApprovalHandler),
                        x => new AskApproval(
                            x.Instance.CorrelationId, 
                            x.Instance.GroupId, 
                            x.Instance.OperatorEmail, 
                            x.Instance.User))
                    .TransitionTo(WaitingForApproval));

            During(WaitingForApproval,
                When(ApprovalReceived)
                    .If(x => x.Data.Approved, binder => binder
                        .Then(x => x.Instance.ProvisioningUserApproved = true)
                        .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.UsersHandler),
                            x => new CreateUser(
                                x.Instance.CorrelationId, 
                                x.Instance.User, 
                                new List<Guid> { x.Instance.GroupId }, 
                                x.Instance.OperatorEmail))
                        .TransitionTo(CreatingUser))
                    .If(x => !x.Data.Approved, binder => binder
                        .Then(x => x.Instance.ErrorMessage += x.Data.Reason)
                        .Publish(x => new NewUserProvisioningWithApprovalCompleted(
                            x.Instance.CorrelationId,
                            x.Instance.GroupId,
                            Guid.Empty,
                            x.Instance.User,
                            x.Instance.OperatorEmail,
                            false,
                            false,
                            x.Instance.ErrorMessage))
                        .TransitionTo(RequestRejected))
                );

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
                    .Publish(x => new NewUserProvisioningWithApprovalCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        x.Instance.ProvisioningUserApproved,
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
                    .Publish(x => new NewUserProvisioningWithApprovalCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        x.Instance.ProvisioningUserApproved,
                        true))
                    .Finalize(),
                When(UserAssignedToGroupUnsuccessfully)
                    .Then(x => x.Instance.ErrorMessage += x.Data.Reason)
                    .Send((instance, data) => new Uri(Constants.SagaPattern.QueueUris.UsersHandler),
                        x => new UnassignGroupsFromUser(x.Instance.CorrelationId, x.Instance.NewUserId, new List<Guid> { x.Instance.GroupId }))
                    .TransitionTo(UnassigningGroupFromUser));

            During(UnassigningGroupFromUser,
                When(GroupsUnassignedFromUserSuccessfully)
                    .Then(x => x.Instance.EndDate = DateTime.UtcNow)
                    .Publish(x => new NewUserProvisioningWithApprovalCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        x.Instance.ProvisioningUserApproved,
                        true,
                        x.Instance.ErrorMessage))
                    .TransitionTo(PartiallyCompleted),
                When(GroupsUnassignedFromUserUnsuccessfully)
                    .Then(x =>
                    {
                        x.Instance.ErrorMessage += x.Data.Reason;
                        x.Instance.EndDate = DateTime.UtcNow;
                    })
                    .Publish(x => new NewUserProvisioningWithApprovalCompleted(
                        x.Instance.CorrelationId,
                        x.Instance.GroupId,
                        x.Instance.AssignedGroupId,
                        x.Instance.User,
                        x.Instance.OperatorEmail,
                        true,
                        x.Instance.ProvisioningUserApproved,
                        x.Instance.ErrorMessage))
                    .TransitionTo(PartiallyCompleted));
        }

        #region Events

        public Event<ProvisionNewUserSingleGroupWithApproval> ProvisionNewUserSingleGroupWithApproval { get; private set; }
        public Event<AskApproval> AskApproval { get; private set; }
        public Event<ApprovalReceived> ApprovalReceived { get; private set; }
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

        public State WaitingForApproval { get; private set; }
        public State CreatingUser { get; private set; }
        public State AssigningUserToGroup { get; private set; }
        public State UnassigningGroupFromUser { get; private set; }
        public State PartiallyCompleted { get; private set; }
        public State RequestRejected { get; private set; }
        public State Failed { get; private set; }

        #endregion
    }

    public class ProvisionUserWithApprovalSagaState : SagaStateMachineInstance, IVersionedSaga
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
        public bool ProvisioningUserApproved { get; set; }
        public string ErrorMessage { get; set; }
    }
}
