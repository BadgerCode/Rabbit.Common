using Rabbit.Common.Connection;
using Rabbit.Common.Models;

namespace Rabbit.Common.Testing.QueueSetup
{
    public class TestQueueRemover
    {
        public static void RemoveQueue(RabbitConfig rabbitConfig, string queueName)
        {
            using (var connectionManager = new RabbitConnectionFactory().Create(rabbitConfig))
            using (var channel = connectionManager.Get().CreateModel())
            {
                channel.QueueDelete(queueName);
            }
        }
    }
}
