using Rabbit.Common.Interfaces.Publishers;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IQueuePublisherFactory<in TMessage, out TErrorResponse>
    {
        IQueuePublisher<TMessage, TErrorResponse> Create();
    }
}
