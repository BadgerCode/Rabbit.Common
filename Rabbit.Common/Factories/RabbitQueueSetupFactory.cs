using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Interfaces.QueueSetup;
using Rabbit.Common.QueueSetup;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Factories
{
    public class RabbitQueueSetupFactory : IRabbitQueueSetupFactory
    {
        private readonly IRabbitConnection _connection;

        public RabbitQueueSetupFactory(IRabbitConnection connection)
        {
            _connection = connection;
        }

        public IRabbitQueueSetup Create()
        {
            return new RabbitQueueSetup(_connection, new RabbitHeaderEncoder());
        }
    }
}
