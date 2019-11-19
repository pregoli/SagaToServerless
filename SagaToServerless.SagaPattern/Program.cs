using GreenPipes;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SagaToServerless.Common;
using SagaToServerless.Data.Repositories;
using SagaToServerless.SagaPattern.Configurations;
using SagaToServerless.SagaPattern.Extensions;
using SagaToServerless.SagaPattern.Handlers;
using SagaToServerless.SagaPattern.Sagas;
using SagaToServerless.Services;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace SagaToServerless.SagaPattern
{
    class Program
    {
     
        private static readonly string mongoDbConnectionString = ConfigurationManager.AppSettings.Get("MongoDb");
        private static readonly string serviceBusConnectionString = ConfigurationManager.AppSettings.Get("ServiceBus");
        private static readonly string sendGridApiKey = ConfigurationManager.AppSettings.Get("SendGridApiKey");
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Configure(serviceCollection, serviceProvider);
        }

        public static void Configure(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            RegisterRepositories(serviceCollection, serviceProvider);
            RegisterServices(serviceCollection, serviceProvider);
            RegisterBus(serviceCollection, serviceProvider);
        }

        private static void RegisterRepositories(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            serviceCollection.AddSingleton<IUserRepository>(x => new UserRepository(mongoDbConnectionString, Constants.CollectionNames.Users));
            serviceCollection.AddSingleton<IGroupRepository>(x => new GroupRepository(mongoDbConnectionString, Constants.CollectionNames.Groups));
            serviceProvider = serviceCollection.BuildServiceProvider();
        }
        
        private static void RegisterServices(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            serviceCollection.AddTransient<IUserService>(x => new UserService(serviceProvider.GetRequiredService<IUserRepository>()));
            serviceCollection.AddTransient<IGroupService>(x => new GroupService(serviceProvider.GetRequiredService<IGroupRepository>()));
            serviceCollection.AddSingleton<ISendGridService>(x => new SendGridService(sendGridApiKey));

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void RegisterBus(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            var busConfiguration = RabbitMqBusConfiguration.Parse(serviceBusConnectionString);

            var provisionUserWithSingleGroupSaga = new ProvisionUserWithSingleGroupSaga();
            var provisionUserWithMultipleGroupsSaga = new ProvisionUserWithMultipleGroupsSaga();
            var provisionUserWithApprovalSaga = new ProvisionUserWithApprovalSaga();

            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(busConfiguration.ToRabbitMqHostSettings());

                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.ProvisionUserWithSingleGroupService, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));

                        configurator.StateMachineSaga(provisionUserWithSingleGroupSaga,
                            new MongoDbSagaRepository<ProvisionUserWithSingleGroupSagaState>(mongoDbConnectionString, "SagaToServerless",
                                Constants.CollectionNames.ProvisionUserWithSingleGroupSagas));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.ProvisionUserWithMultipleGroupsService, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));

                        configurator.StateMachineSaga(provisionUserWithMultipleGroupsSaga,
                            new MongoDbSagaRepository<ProvisionUserWithMultipleGroupsSagaState>(mongoDbConnectionString, "SagaToServerless",
                                Constants.CollectionNames.ProvisionUserWithMultipleGroupSagas));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.ProvisionUserWithApprovalService, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));

                        configurator.StateMachineSaga(provisionUserWithApprovalSaga,
                            new MongoDbSagaRepository<ProvisionUserWithApprovalSagaState>(mongoDbConnectionString, "SagaToServerless",
                                Constants.CollectionNames.ProvisionUserWithApprovalSagas));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.UsersHandlerQueue, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                        
                        configurator.Consumer(() => new UsersHandler(serviceProvider.GetRequiredService<IUserService>()));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.GroupsHandlerQueue, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                        
                        configurator.Consumer(() => new GroupsHandler(serviceProvider.GetRequiredService<IGroupService>()));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.NotificationsHandlerQueue, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                        
                        configurator.Consumer(() => new NotificationsHandler(serviceProvider.GetRequiredService<ISendGridService>()));
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.ProvisionUserHandlerQueue, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                        
                        configurator.Consumer(() => new ProvisionUserHandler());
                    });
                sbc.ReceiveEndpoint(host, Constants.SagaPattern.QueueNames.ApprovalHandlerQueue, configurator =>
                    {
                        configurator.UseRetry(retryConfigurator =>
                            retryConfigurator.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                        
                        configurator.Consumer(() => new ApprovalHandler());
                    });
            });

            bus.Start();
            serviceCollection.AddSingleton<IBus>(bus);
            serviceCollection.AddSingleton<IBusControl>(bus);

            serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
