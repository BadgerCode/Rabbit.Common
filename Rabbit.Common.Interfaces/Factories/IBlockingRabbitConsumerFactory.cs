using Rabbit.Common.Interfaces.Consumers;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IBlockingRabbitConsumerFactory<TMessage>
    {
        IBlockingRabbitConsumer<TMessage> CreateQueueConsumer(string queueName);
    }
}