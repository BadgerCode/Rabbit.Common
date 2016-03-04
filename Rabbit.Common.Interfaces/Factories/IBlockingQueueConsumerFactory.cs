using Rabbit.Common.Interfaces.Consumers;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IBlockingQueueConsumerFactory<out TMessage>
    {
        IBlockingQueueConsumer<TMessage> CreateQueueConsumer(string queueName);
    }
}