# SagaToServerless

## Overview

This repository contains an example of workflow runs by adopting the <b>Saga Pattern</b> from a side and <b>Durable functions</b> from other.
</br></br>The workflow contains the following steps:
- Start user provisioning
- Create user with a group reference
- Assign the user to the group
- Send an email to the operator started the provisioning workflow

## Scenarios
 1. If the user cannot be created, the workflow will fail.
 2. If the user assignment to the group will fail, the group reference will be removed from the previously created user, as compensation action.

## Before to get started
1. Insaltt ***.Net Core 2+*** on your local machine.
1. Create/Enable ***RabbitMQ*** to run locally.
2. Install ***MongoDB*** and make it run locally.
3. Create a test ***SendGrid*** account.

## Get started
Clone the repository.

**DURABLE FUNCTIONS**: Run the project ***SagaToServerless.Durable*** locally from Visual Studio 2019.</br>
Create a file in the root of the ***SagaToServerless.Durable*** project named local.settings.json and populate it as shown below:
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

If you prefer to use the storage emulator, you can then use the value ***UseDevelopmentStorage=true*** for the key **AzureWebJobsStorage**
