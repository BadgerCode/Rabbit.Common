using Rabbit.Common.Interfaces.Connection;
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
        private readonly IRabbitConnection _connection;

        public RabbitQueueSetupFactory(IRabbitConnection connection)
        {
            _connection = connection;
        }

        public RabbitQueueSetup Create()
        {
            return new RabbitQueueSetup(_connection, new RabbitHeaderEncoder());
        }
    }
}
