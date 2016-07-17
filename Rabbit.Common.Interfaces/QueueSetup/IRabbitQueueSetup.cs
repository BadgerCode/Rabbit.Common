using System.Collections.Generic;

namespace Rabbit.Common.Interfaces.QueueSetup
{
    public interface IRabbitQueueSetup
    {
        void CreateHeaderExchangeQueue(string exchangeName, string queueName, IDictionary<string, string> headerBindings, bool allHeaderBindingsMustBeTrue = true);
        void CreateQueue(string exchangeName, string queueName);
    }
}