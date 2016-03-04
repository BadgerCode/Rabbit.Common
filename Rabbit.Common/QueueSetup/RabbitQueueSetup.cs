using System.Collections.Generic;
using Rabbit.Common.Connection;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.QueueSetup
{
    public interface IRabbitQueueSetup
    {
        void CreateHeaderExchangeQueue(string exchangeName, string queueName, IDictionary<string, string> headerBindings, bool allHeaderBindingsMustBeTrue = true);
        void CreateQueue(string exchangeName, string queueName);
    }

    public class RabbitQueueSetup : IRabbitQueueSetup
    {
        private readonly RabbitConnection _connection;
        private readonly IRabbitHeaderEncoder _rabbitHeaderEncoder;

        public RabbitQueueSetup(RabbitConnection connection, IRabbitHeaderEncoder rabbitHeaderEncoder)
        {
            _connection = connection;
            _rabbitHeaderEncoder = rabbitHeaderEncoder;
        }

        public void CreateQueue(string exchangeName, string queueName)
        {
            using (var channel = _connection.Get().CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);

                channel.QueueBind(queueName, exchangeName, string.Empty);
            }
        }

        public void CreateHeaderExchangeQueue(string exchangeName, string queueName, IDictionary<string, string> headerBindings,
                                              bool allHeaderBindingsMustBeTrue = true)
        {
            using (var channel = _connection.Get().CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);

                var headers = new Dictionary<string, string>(headerBindings);
                headers.Remove("x-match");
                headers.Add("x-match", allHeaderBindingsMustBeTrue ? "all" : "any");

                channel.QueueBind(queueName, exchangeName, string.Empty, _rabbitHeaderEncoder.Encode(headers));
            }
        }
    }
}