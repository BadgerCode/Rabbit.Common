using Rabbit.Common.Interfaces.Connection;
using RabbitMQ.Client;

namespace Rabbit.Common.Factories
{
    public interface IQueueingBasicConsumerFactory
    {
        QueueingBasicConsumer Create(string queueName);
    }

    public class QueueingBasicConsumerFactory : IQueueingBasicConsumerFactory
    {
        private readonly IRabbitConnection _rabbitConnection;

        public QueueingBasicConsumerFactory(IRabbitConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
        }

        public QueueingBasicConsumer Create(string queueName)
        {
            var channel = _rabbitConnection.Get().CreateModel();
            channel.BasicQos(0, 100, false);

            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);

            return consumer;
        }
    }
}