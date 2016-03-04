using Rabbit.Common.Interfaces.Connection;
using RabbitMQ.Client;

namespace Rabbit.Common.Connection
{
    public class RabbitConnection : IQueueConnection<IConnection>
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private readonly object _connectionLock = new object();
        private readonly object _abortLock = new object();

        public RabbitConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Connect()
        {
            Get();
        }

        public IConnection Get()
        {
            lock (_connectionLock)
            {
                if (_connection != null && _connection.IsOpen)
                {
                    return _connection;
                }

                _connection = _connectionFactory.CreateConnection();
            }

            return _connection;
        }

        public void Abort()
        {
            lock (_abortLock)
            {
                if (_connection == null)
                {
                    return;
                }

                _connection.Abort();
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            Abort();
        }
    }
}