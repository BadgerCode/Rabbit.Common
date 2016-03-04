using Rabbit.Common.Consumers;
using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Consumers;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Factories
{
    public class BlockingRabbitConsumerFactory<TMessageBody> : IBlockingRabbitConsumerFactory<TMessageBody>
    {
        private readonly IRabbitConnection _connection;

        public BlockingRabbitConsumerFactory(IRabbitConnection connection)
        {
            _connection = connection;
        }

        public IBlockingRabbitConsumer<TMessageBody> CreateQueueConsumer(string queueName)
        {
            var queueingBasicConsumer = new QueueingBasicConsumerFactory(_connection).Create(queueName);

            return new BlockingRabbitConsumer<TMessageBody>(_connection, queueingBasicConsumer, new RabbitHeaderEncoder(), new RabbitBodyEncoder<TMessageBody>());
        }
    }
}
