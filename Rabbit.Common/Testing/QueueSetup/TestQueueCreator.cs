using System.Collections.Generic;
using Rabbit.Common.Connection;
using Rabbit.Common.Factories;
using Rabbit.Common.Models;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Testing.QueueSetup
{
    public class TestQueueCreator
    {
        public static void CreateHeaderExchangeQueue(RabbitConfig rabbitConfig, string exchangeName, string queueName, IDictionary<string, string> headerBindings,
                                                     bool allHeaderBindingsMustBeTrue = true)
        {
            CreateHeaderExchangeQueue(rabbitConfig, exchangeName, queueName, headerBindings, string.Empty, allHeaderBindingsMustBeTrue);
        }

        public static void CreateHeaderExchangeQueue(RabbitConfig rabbitConfig, string exchangeName, string queueName, IDictionary<string, string> headerBindings, string routingKey,
                                                     bool allHeaderBindingsMustBeTrue = true)
        {
            using(var connectionManager = new RabbitConnectionFactory().Create(rabbitConfig))
            using (var channel = connectionManager.Get().CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);

                var headers = new Dictionary<string, string>(headerBindings);
                headers.Remove("x-match");
                headers.Add("x-match", allHeaderBindingsMustBeTrue ? "all" : "any");

                channel.QueueBind(queueName, exchangeName, routingKey ?? string.Empty, new RabbitHeaderEncoder().Encode(headers));
            }   
        }
    }
}
