using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Models;

namespace Rabbit.Common.Interfaces.Factories
{
    public interface IRabbitConnectionFactory
    {
        IRabbitConnection Create(RabbitConfig rabbitConfig);
    }
}