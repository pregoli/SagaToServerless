using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.SagaPattern.Configurations
{
    public class RabbitMqBusConfiguration
    {
        public static class QueryParameters
        {
            public readonly static string UseSsl = "usessl";
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string Endpoint { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        private string Protocol { get; set; }

        public static string GetAmqpFormat(RabbitMqBusConfiguration rabbitMqBusConfiguration)
        {
            return rabbitMqBusConfiguration.Endpoint.Replace("rabbitmq://", "amqp://");
        }

        public static RabbitMqBusConfiguration Parse(string connectionString)
        {
            var protocol = connectionString.Substring(0, connectionString.IndexOf("://"));
            var uri = new Uri(connectionString);
            var query = ParseQuery(uri);

            return new RabbitMqBusConfiguration
            {
                Username = uri.UserInfo.Split(':')[0],
                Password = uri.UserInfo.Split(':')[1],
                VirtualHost = uri.AbsolutePath.Substring(1) == string.Empty ? "/" : uri.AbsolutePath.Substring(1),
                Endpoint = connectionString,
                Host = uri.Host,
                Protocol = protocol,
                Port = uri.Port,
                UseSsl = GetSslValueFromQueryDictionary(query)

            };
        }

        private static bool GetSslValueFromQueryDictionary(Dictionary<string, string> query)
        {
            bool sslValue = false;

            if (!query.ContainsKey(QueryParameters.UseSsl))
                return sslValue;

            if (!bool.TryParse(query[QueryParameters.UseSsl], out sslValue))
                return sslValue;

            return sslValue;
        }

        private static Dictionary<string, string> ParseQuery(Uri uri)
        {
            var param = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(uri.Query))
                return param;

            var query = uri.Query.Substring(1); //remove ? char
            var keyValue = query.Split('&');
            foreach (var item in keyValue)
            {
                var key = item.Split('=')[0].ToLower();
                var value = item.Split('=')[1];
                param.Add(key, value);
            }
            return param;
        }


        public static Uri GetSchedulerAddress(RabbitMqBusConfiguration rabbitMqBusConfiguration, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));
            if (rabbitMqBusConfiguration == null)
                throw new ArgumentNullException(nameof(rabbitMqBusConfiguration));
            if (rabbitMqBusConfiguration.VirtualHost == "/")
                return new Uri($"{rabbitMqBusConfiguration.Protocol}://{rabbitMqBusConfiguration.Host}:{rabbitMqBusConfiguration.Port}/{queueName}");

            return new Uri($"{rabbitMqBusConfiguration.Protocol}://{rabbitMqBusConfiguration.Host}:{rabbitMqBusConfiguration.Port}/{rabbitMqBusConfiguration.VirtualHost}/{queueName}");
        }
    }
}
