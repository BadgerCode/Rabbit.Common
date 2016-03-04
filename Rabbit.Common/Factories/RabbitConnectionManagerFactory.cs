using Rabbit.Common.Connection;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Models;
using RabbitMQ.Client;

namespace Rabbit.Common.Factories
{
    public class RabbitConnectionFactory : IQueueConnectionFactory<RabbitConnection, RabbitConfig>
    {
        public RabbitConnection Create(RabbitConfig rabbitConfig)
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

            return new RabbitConnection(connectionFactory);
        }
    }
}