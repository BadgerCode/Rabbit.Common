using Rabbit.Common.Interfaces.QueueSetup;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IRabbitQueueSetupFactory
    {
        IRabbitQueueSetup Create();
    }
}