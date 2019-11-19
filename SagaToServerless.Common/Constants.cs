using System;
using System.Configuration;

namespace SagaToServerless.Common
{
    public static class Constants
    {
        public static class CollectionNames
        {
            public const string Users = "Users";
            public const string Groups = "Groups";
            public const string ProvisionUserWithSingleGroupSagas = "ProvisionUserWithSingleGroupSagas";
            public const string ProvisionUserWithMultipleGroupSagas = "ProvisionUserWithMultipleGroupSagas";
            public const string ProvisionUserWithApprovalSagas = "ProvisionUserWithApprovalSagas";
        }

        public static class FunctionNames
        {
            public static class Trigger
            {
                public const string TriggerUserProvisioningChainingWorkflow = "TriggerUserProvisioningChainingWorkflow_T";
                public const string TriggerUserProvisioningFanInFanOutWorkflow = "TriggerUserProvisioningFanInFanOutWorkflow_T";
                public const string TriggerUserProvisioningApprovalWorkflow = "TriggerUserProvisioningApprovalWorkflow_T";
                public const string TriggerHttpApprovalProcessor = "TriggerHttpApprovalProcessor_T";
            }

            public static class Orchestrator
            {
                public const string StartProvisionUserWithSingleGroupOrchestrator = "StartProvisionUserWithSingleGroup_O";
                public const string StartProvisionUserWithMultipleGroupsOrchestrator = "StartProvisionUserWithMultipleGroups_O";
                public const string ExecuteUserProvisioninApprovalgWorkflow = "ExecuteUserProvisioninApprovalgWorkflow_O";
            }

            public static class Activity
            {
                public const string CreateUser = "CreateUser_A";
                public const string AssignUserToGroup = "AssignUserToGroup_A";
                public const string AskUserCreationApproval = "AskUserCreationApproval_A";
                public const string SendEmail = "SendEmail_A";
            }
        }

        public static class SagaPattern
        {
            public static class QueueNames
            {
                public const string ProvisionUserWithSingleGroupService = "provisionuserwithsinglegroupservice";
                public const string ProvisionUserWithMultipleGroupsService = "provisionuserwithmultiplegroupsservice";
                public const string ProvisionUserWithApprovalService = "provisionuserwithapprovalservice";
                public const string UsersHandlerQueue = "usershandlerqueue";
                public const string GroupsHandlerQueue = "groupshandlerqueue";
                public const string NotificationsHandlerQueue = "notificationshandlerqueue";
                public const string ApprovalHandlerQueue = "approvalhandlerqueue";
                public const string ProvisionUserHandlerQueue = "provisionuserhandlerqueue";
            }

            public static class QueueUris
            {
                public static readonly string UsersHandler = $"rabbitmq://{ConfigurationManager.AppSettings.Get("MassTransit.Endpoints.Host")}/{QueueNames.UsersHandlerQueue}";
                public static readonly string GroupsHandler = $"rabbitmq://{ConfigurationManager.AppSettings.Get("MassTransit.Endpoints.Host")}/{QueueNames.GroupsHandlerQueue}";
                public static readonly string NotificationsHandler = $"rabbitmq://{ConfigurationManager.AppSettings.Get("MassTransit.Endpoints.Host")}/{QueueNames.NotificationsHandlerQueue}";
                public static readonly string ApprovalHandler = $"rabbitmq://{ConfigurationManager.AppSettings.Get("MassTransit.Endpoints.Host")}/{QueueNames.ApprovalHandlerQueue}";
            }
        }
    }
}
