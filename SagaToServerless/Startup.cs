using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SagaToServerless.Data.Repositories;
using SagaToServerless.Services;
using SagaToServerless.Common;
using System;
using SagaToServerless.Durable;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SagaToServerless.Durable
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var mongoConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:mongoDb");
            var sendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IUserRepository>(new UserRepository(mongoConnectionString, Constants.CollectionNames.Users));
            builder.Services.AddSingleton<IGroupRepository>(new GroupRepository(mongoConnectionString, Constants.CollectionNames.Groups));
            builder.Services.AddSingleton<ISendGridService>(new SendGridService(sendGridApiKey));
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IGroupService, GroupService>();
        }
    }
}
