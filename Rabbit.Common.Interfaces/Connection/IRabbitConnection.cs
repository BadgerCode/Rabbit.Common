using System;
using RabbitMQ.Client;

namespace Rabbit.Common.Interfaces.Connection
{
    public interface IRabbitConnection : IDisposable
    {
        IConnection Get();
        void Abort();
    }
}