# SagaToServerless

## Overview

This repository contains an example of workflow runs by adopting the [**Saga Pattern**](https://masstransit-project.com/usage/sagas/) from a side and [**Durable functions**](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview) from other.
</br></br>The workflow contains the following steps:
- Start user provisioning
- Create user with a group reference
- Assign the user to the group
- Send an email to the operator started the provisioning workflow

## Scenarios
 1. If the user cannot be created, the workflow will fail.
 2. If the user assignment to the group will fail, the group reference will be removed from the previously created user, as compensation action.

## Before to get started
1. Install [**.Net Core 2.1**](https://dotnet.microsoft.com/download/dotnet-core/2.1) on your local machine.
1. Create/Enable [**RabbitMQ**](https://www.rabbitmq.com/download.html) to run locally.
2. Install [**MongoDB**](https://docs.mongodb.com/manual/installation/) and make it run locally.
3. Create a test [**SendGrid**](https://sendgrid.com/) account.

## Get started
Clone the repository.

**DURABLE FUNCTIONS**: Run the project [***SagaToServerless.Durable***](https://github.com/pregoli/SagaToServerless/tree/master/SagaToServerless) locally from Visual Studio 2019.</br>
Create a file in the root of the [***SagaToServerless.Durable***](https://github.com/pregoli/SagaToServerless/tree/master/SagaToServerless) project named local.settings.json and populate it as shown below:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "{your azure storage connection string}",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SendGridApiKey": "{your sendgrid api key}"
  },
  "ConnectionStrings": {
    "mongoDb": "mongodb://localhost:27017/SagaToServerless?maxPoolSize=150",
    "rabbitMQ": "amqp://rabbitmq:rabbitmq@localhost:{port}"
  }
}
```

If you prefer to use the storage emulator, you can use the value ***UseDevelopmentStorage=true*** for the key **AzureWebJobsStorage**.

You will find three Orchestrators in the project:

1. ProvisionUserWithSingleGroupOrchestrator: It will be started from the trigger function [***TriggerUserProvisioningChainingWorkflow***](https://github.com/pregoli/SagaToServerless/blob/master/SagaToServerless/Triggers/NewUserProvisioningTrigger.cs) listening from the **chainingqueue** on RabbitMQ.

2. ProvisionUserWithMultipleGroupsOrchestrator: It will be started from the trigger function [***TriggerUserProvisioningFanInFanOutWorkflow***](https://github.com/pregoli/SagaToServerless/blob/master/SagaToServerless/Triggers/NewUserProvisioningTrigger.cs) listening from the **faninfanoutqueue** on RabbitMQ.

3. ProvisionUserOrchestratorApproval: It will be started from the trigger function [***TriggerUserProvisioningApprovalWorkflow***](https://github.com/pregoli/SagaToServerless/blob/master/SagaToServerless/Triggers/NewUserProvisioningTrigger.cs) listening from the **approvalqueue** on RabbitMQ.

You can find example of commands for durable functions to send from RabbitMQ in the project [**SagaToServerless.Common**](https://github.com/pregoli/SagaToServerless/tree/master/SagaToServerless.Common), file [***utils.json***](https://github.com/pregoli/SagaToServerless/blob/master/SagaToServerless.Common/utils.json)
