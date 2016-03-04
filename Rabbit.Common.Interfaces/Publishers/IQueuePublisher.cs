using System;

namespace Rabbit.Common.Interfaces.Publishers
{
    public interface IQueuePublisher<in TMessage, out TErrorResponse>
    {
        void Publish(TMessage message, Action<TErrorResponse> onFailure = null);
    }
}
