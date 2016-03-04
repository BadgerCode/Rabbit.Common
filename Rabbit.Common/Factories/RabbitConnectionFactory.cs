using Rabbit.Common.Connection;
using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Interfaces.Models;
using RabbitMQ.Client;

namespace Rabbit.Common.Factories
{
    public class RabbitConnectionFactory : IRabbitConnectionFactory
    {
        public IRabbitConnection Create(RabbitConfig rabbitConfig)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = rabbitConfig.Host,
                UserName = rabbitConfig.Username,
                Password = rabbitConfig.Password,
                Port = rabbitConfig.Port,
                VirtualHost = rabbitConfig.VirtualHost,
                ClientProperties = { { "originating_service", rabbitConfig.ServiceName } }
            };

            var connection = new RabbitConnection(connectionFactory);
            connection.Get();

            return connection;
        }
    }
}
