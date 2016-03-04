using NUnit.Framework;
using Rabbit.Common.Connection;
using Rabbit.Common.Factories;
using RabbitMQ.Client;

namespace Rabbit.Common.AcceptanceTests.ConnectionTests
{
    [TestFixture]
    public class ConnectionTests
    {
        [TestFixture]
        public class GivenAConnectionAttempt
        {
            private IConnection _result;
            private RabbitConnection _connection;

            [OneTimeSetUp]
            public void WhenConnectingToRabbit()
            {
                _connection = new RabbitConnectionFactory().Create(Configuration.RabbitConfig);
                _connection.Connect();
                _result = _connection.Get();
            }

            [Test]
            public void ThenTheConnectionIsOpened()
            {
                Assert.That(_result.IsOpen, Is.True);
            }

            [OneTimeTearDown]
            public void TearItDown()
            {
                _connection.Abort();
            }
        }
    }
}
