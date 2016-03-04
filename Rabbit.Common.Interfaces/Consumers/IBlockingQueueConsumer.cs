namespace Rabbit.Common.Interfaces.Consumers
{
    public interface IBlockingQueueConsumer<out TMessage>
    {
        TMessage Dequeue();
    }
}