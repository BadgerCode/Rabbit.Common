using System;

namespace Rabbit.Common.Interfaces.Connection
{
    public interface IQueueConnection<out TConnection> : IDisposable
    {
        void Connect();
        TConnection Get();
        void Abort();
    }
}