using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Publishers;
using Rabbit.Common.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Rabbit.Common.Tests.PublisherTests
{
    [TestFixture]
    public class PublishTests
    {
        public class TestMessageModel
        {
            public string SomeProperty { get; set; }
            public int AnIntProperty { get; set; }
        }

        [TestFixture]
        public class GivenAMessageToPublish
        {
            private Mock<IModel> _channel;
            private string _exchangeName;
            private Mock<IBasicProperties> _basicProperties;
            private Dictionary<string, object> _encodedHeaders;
            private byte[] _encodedBody;
            private IDictionary<string, object> _attachedHeaders;

            [OneTimeSetUp]
            public void WhenPublishingTheMessage()
            {
                var rabbitMessage = new RabbitMessage<TestMessageModel>(new Dictionary<string, string>(), new TestMessageModel());
                _encodedBody = new byte[] {1,2,3};
                _encodedHeaders = new Dictionary<string, object> { { "key", 5 } };

                _exchangeName = "exchange name";

                var connectionManager = new Mock<IRabbitConnection>();
                var connection = new Mock<IConnection>();
                _channel = new Mock<IModel>();
                _basicProperties = new Mock<IBasicProperties>();

                connectionManager.Setup(m => m.Get()).Returns(connection.Object);
                connection.Setup(c => c.CreateModel()).Returns(_channel.Object);
                _channel.Setup(c => c.CreateBasicProperties()).Returns(_basicProperties.Object);

                _basicProperties
                    .SetupSet(p => p.Headers = It.IsAny<IDictionary<string, object>>())
                    .Callback((IDictionary<string, object> headers) => { _attachedHeaders = _encodedHeaders; });

                var headerEncoder = new Mock<IRabbitHeaderEncoder>();
                headerEncoder.Setup(e => e.Encode(rabbitMessage.Headers)).Returns(_encodedHeaders);

                var bodyEncoder = new Mock<IRabbitBodyEncoder<TestMessageModel>>();
                bodyEncoder.Setup(e => e.Encode(rabbitMessage.Body)).Returns(_encodedBody);

                var publisher = new RabbitQueuePublisher<TestMessageModel>(connectionManager.Object, _exchangeName, headerEncoder.Object, bodyEncoder.Object );
                publisher.Publish(rabbitMessage);
            }

            [Test]
            public void ThenTheEncodedHeadersAreAttached()
            {
                Assert.That(_attachedHeaders, Is.EqualTo(_encodedHeaders));
            }

            [Test]
            public void ThenTheMessageIsPublished()
            {
                _channel.Verify(channel => channel.BasicPublish(_exchangeName, string.Empty, true, _basicProperties.Object, _encodedBody));
            }
        }

        [TestFixture]
        public class GivenTheMessageFailsToPublish
        {
            private FailedRabbitMessage<TestMessageModel> _failedMessage;
            private Dictionary<string, string> _decodedFailedMessageHeaders;
            private TestMessageModel _decodedFailedMessageBody;
            private BasicReturnEventArgs _failedMessageEventArgs;

            [OneTimeSetUp]
            public void WhenPublishingTheMessage()
            {
                var rabbitMessage = new RabbitMessage<TestMessageModel>(new Dictionary<string, string>(), new TestMessageModel());

                var failedMessageHeaders = new Dictionary<string, object> { { "sdwfw", 5 } };
                var failedMessageBody = new byte[] { 1, 2, 3 };
                _failedMessageEventArgs = new BasicReturnEventArgs
                {
                    BasicProperties = new BasicProperties { Headers = failedMessageHeaders },
                    Body = failedMessageBody,
                    ReplyCode = 6465,
                    ReplyText = "ffffefe"
                };
                _decodedFailedMessageHeaders = new Dictionary<string, string> { { "decodedheaderkey", "decodedheadervalue" } };
                _decodedFailedMessageBody = new TestMessageModel();

                var connectionManager = new Mock<IRabbitConnection>();
                var connection = new Mock<IConnection>();
                var channel = new Mock<IModel>();
                connectionManager.Setup(m => m.Get()).Returns(connection.Object);
                connection.Setup(c => c.CreateModel()).Returns(channel.Object);
                channel.Setup(c => c.CreateBasicProperties()).Returns(new Mock<IBasicProperties>().Object);

                channel
                    .Setup(c =>c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), 
                                              It.IsAny<bool>(),It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                    .Raises(f => f.BasicReturn += null, _failedMessageEventArgs);

                var headerEncoder = new Mock<IRabbitHeaderEncoder>();
                headerEncoder.Setup(e => e.Decode(failedMessageHeaders)).Returns(_decodedFailedMessageHeaders);

                var bodyEncoder = new Mock<IRabbitBodyEncoder<TestMessageModel>>();
                bodyEncoder.Setup(e => e.Decode(failedMessageBody)).Returns(_decodedFailedMessageBody);

                var publisher = new RabbitQueuePublisher<TestMessageModel>(connectionManager.Object, "exchange name", headerEncoder.Object, bodyEncoder.Object);
                publisher.Publish(rabbitMessage, failedMessage =>
                {
                    _failedMessage = failedMessage;
                });
            }

            [Test]
            public void ThenTheFailureCallbackIsInvokedWithTheFailedMessage()
            {
                Assert.That(_failedMessage, Is.Not.Null, "Failed message was not sent to message publishing failure callback.");
                Assert.That(_failedMessage.Response.ReplyCode, Is.EqualTo(_failedMessageEventArgs.ReplyCode));
                Assert.That(_failedMessage.Response.ReplyText, Is.EqualTo(_failedMessageEventArgs.ReplyText));

                Assert.That(_failedMessage.Message.Headers, Is.EqualTo(_decodedFailedMessageHeaders));
                Assert.That(_failedMessage.Message.Body, Is.EqualTo(_decodedFailedMessageBody));
            }
        }
    }
}
