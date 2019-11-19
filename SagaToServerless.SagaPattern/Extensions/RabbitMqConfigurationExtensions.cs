using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configurators;
using SagaToServerless.SagaPattern.Configurations;

namespace SagaToServerless.SagaPattern.Extensions
{
    public static class RabbitMqConfigurationExtensions
    {
        public static RabbitMqHostSettings ToRabbitMqHostSettings(this RabbitMqBusConfiguration configuration)
        {
            var hostConfiguration = new RabbitMqHostConfigurator(configuration.Host, configuration.VirtualHost, (ushort)configuration.Port)
            {
                PublisherConfirmation = true
            };

            if (!string.IsNullOrEmpty(configuration.Username))
                hostConfiguration.Username(configuration.Username);
            if (!string.IsNullOrEmpty(configuration.Password))
                hostConfiguration.Password(configuration.Password);
            if (configuration.UseSsl)
                hostConfiguration.UseSsl((x) => { x.Protocol = System.Security.Authentication.SslProtocols.Tls12; });

            return hostConfiguration.Settings;
        }
    }
}
