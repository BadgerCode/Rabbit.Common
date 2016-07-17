using Rabbit.Common.Factories;
using Rabbit.Common.Interfaces.Models;

namespace Rabbit.Common.Testing.QueueSetup
{
    public class TestQueueRemover
    {
        public static void RemoveQueue(RabbitConfig rabbitConfig, string queueName)
        {
            using (var channel = new RabbitConnectionFactory().CreateAndConnect(rabbitConfig).Get().CreateModel())
            {
                channel.QueueDelete(queueName);
            }
        }
    }
}
