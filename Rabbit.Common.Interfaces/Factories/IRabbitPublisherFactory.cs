using Rabbit.Common.Interfaces.Publishers;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IRabbitPublisherFactory<TMessageBody>
    {
        IRabbitPublisher<TMessageBody> Create();
    }
}
