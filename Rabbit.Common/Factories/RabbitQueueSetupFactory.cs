using Rabbit.Common.Connection;
using Rabbit.Common.QueueSetup;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Factories
{
    public interface IRabbitQueueSetupFactory
    {
        RabbitQueueSetup Create();
    }

    public class RabbitQueueSetupFactory : IRabbitQueueSetupFactory
    {
        private readonly RabbitConnection _connection;

        public RabbitQueueSetupFactory(RabbitConnection connection)
        {
            _connection = connection;
        }

        public RabbitQueueSetup Create()
        {
            return new RabbitQueueSetup(_connection, new RabbitHeaderEncoder());
        }
    }
}
