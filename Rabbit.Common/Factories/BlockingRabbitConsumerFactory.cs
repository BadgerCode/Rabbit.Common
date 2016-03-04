using Rabbit.Common.Connection;
using Rabbit.Common.Consumers;
using Rabbit.Common.Interfaces.Consumers;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Models;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Factories
{
    public class BlockingRabbitConsumerFactory<TMessageBody> : IBlockingQueueConsumerFactory<RabbitMessage<TMessageBody>>
    {
        private readonly RabbitConnection _connection;

        public BlockingRabbitConsumerFactory(RabbitConnection connection)
        {
            _connection = connection;
        }

        public IBlockingQueueConsumer<RabbitMessage<TMessageBody>> CreateQueueConsumer(string queueName)
        {
            var queueingBasicConsumer = new QueueingBasicConsumerFactory(_connection).Create(queueName);

            return new BlockingRabbitConsumer<TMessageBody>(_connection, queueingBasicConsumer, new RabbitHeaderEncoder(), new RabbitBodyEncoder<TMessageBody>());
        }
    }
}
