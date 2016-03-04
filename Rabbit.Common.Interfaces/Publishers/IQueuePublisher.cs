using System;
using Rabbit.Common.Interfaces.Models;

namespace Rabbit.Common.Interfaces.Publishers
{
    public interface IQueuePublisher<TMessageBody>
    {
        void Publish(RabbitMessage<TMessageBody> message, Action<FailedRabbitMessage<TMessageBody>> onFailure = null);
    }
}
