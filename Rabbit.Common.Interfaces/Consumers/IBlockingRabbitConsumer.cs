using Rabbit.Common.Interfaces.Models;

namespace Rabbit.Common.Interfaces.Consumers
{
    public interface IBlockingRabbitConsumer<TMessage>
    {
        RabbitMessage<TMessage> Dequeue();
    }
}