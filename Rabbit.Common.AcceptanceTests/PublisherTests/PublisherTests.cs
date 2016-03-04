using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rabbit.Common.Factories;
using Rabbit.Common.Models;
using Rabbit.Common.Testing.Consumers;

namespace Rabbit.Common.AcceptanceTests.PublisherTests
{
    [TestFixture]
    public class PublisherTests
    {
        public class TestMessageModel
        {
            public string SomeProperty { get; set; }
            public int AnIntProperty { get; set; }
        }

        public static void AssertTestMessageEquality(RabbitMessage<TestMessageModel> actualMessage, RabbitMessage<TestMessageModel> expectedMessage)
        {
            CollectionAssert.AreEquivalent(expectedMessage.Headers, actualMessage.Headers);

            Assert.That(actualMessage.Body.AnIntProperty, Is.EqualTo(expectedMessage.Body.AnIntProperty));
            Assert.That(actualMessage.Body.SomeProperty, Is.EqualTo(expectedMessage.Body.SomeProperty));
        }

        [TestFixture]
        public class GivenAMessageToPublish
        {
            private List<RabbitMessage<TestMessageModel>> _publishedMessages;
            private List<FailedRabbitMessage<TestMessageModel>> _failedMessages;
            private List<RabbitMessage<TestMessageModel>> _receivedMessages;

            [OneTimeSetUp]
            public void WhenPublishingTheMessage()
            {
                var routingRules = new Dictionary<string, string> { { "RoutingHeader", "Value" + Guid.NewGuid() } };

                _failedMessages = new List<FailedRabbitMessage<TestMessageModel>>();
                _receivedMessages = new List<RabbitMessage<TestMessageModel>>();

                _publishedMessages = new List<RabbitMessage<TestMessageModel>>
                {
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string> (routingRules)
                        {
                            { "aHeaderKey", "aHeaderValue" },
                            { "anotherKey", "anotherValue" }
                        }, 
                        new TestMessageModel { AnIntProperty = 5, SomeProperty = "ehrghr" }
                    ),
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string> (routingRules)
                        {
                            { "bHeaderKey", "bHeaderValue" },
                            { "yetanotherKey", "yetanotherValue" },
                        },
                        new TestMessageModel { AnIntProperty = 23, SomeProperty = "we444" }
                    )
                };

                var testConsumer = TestMessageConsumer<TestMessageModel>.CreateWithTempQueueAndStart(Configuration.RabbitConfig, Configuration.TestExchange, routingRules);

                var connection = new RabbitConnectionFactory().Create(Configuration.RabbitConfig);

                var publisher = new RabbitPublisherFactory<TestMessageModel>(connection, Configuration.TestExchange).Create();
                
                foreach (var publishedMessage in _publishedMessages)
                {
                    publisher.Publish(publishedMessage, message => _failedMessages.Add(message));
                }

                // Give the system a little time to process the message (and possibly fail to route it)
                Thread.Sleep(1000);

                _receivedMessages.Add(testConsumer.TryGetMessage(TimeSpan.FromSeconds(1)));
                _receivedMessages.Add(testConsumer.TryGetMessage(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void ThenTheMessagesAreQueued()
            {
                AssertTestMessageEquality(_receivedMessages[0], _publishedMessages[0]);
                AssertTestMessageEquality(_receivedMessages[1], _publishedMessages[1]);
            }

            [Test]
            public void ThenNoMessagesFail()
            {
                Assert.That(_failedMessages.Count, Is.EqualTo(0));
            }
        }

        [TestFixture]
        public class GivenMultiplePublishersOnTheSameConnection
        {
            private List<RabbitMessage<TestMessageModel>> _publishedMessages;
            private List<FailedRabbitMessage<TestMessageModel>> _failedMessages;
            private List<RabbitMessage<TestMessageModel>> _receivedMessages;

            [OneTimeSetUp]
            public void WhenPublishingTheMessage()
            {
                var routingValue = Guid.NewGuid().ToString();

                _failedMessages = new List<FailedRabbitMessage<TestMessageModel>>();
                _receivedMessages = new List<RabbitMessage<TestMessageModel>>();

                _publishedMessages = new List<RabbitMessage<TestMessageModel>>
                {
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>
                        {
                            { "aHeaderKey", "aHeaderValue" },
                            { "anotherKey", "anotherValue" },
                            { "RoutingHeader", routingValue }
                        },
                        new TestMessageModel { AnIntProperty = 5, SomeProperty = "ehrghr" }
                    ),
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>
                        {
                            { "bHeaderKey", "bHeaderValue" },
                            { "yetanotherKey", "yetanotherValue" },
                            { "RoutingHeader", routingValue }
                        },
                        new TestMessageModel { AnIntProperty = 23, SomeProperty = "we444" }
                    )
                };

                var routingRules = new Dictionary<string, string> { { "RoutingHeader", routingValue } };
                var testConsumer = TestMessageConsumer<TestMessageModel>.CreateWithTempQueueAndStart(Configuration.RabbitConfig, Configuration.TestExchange, routingRules);

                var publisherConnectionManager = new RabbitConnectionFactory().Create(Configuration.RabbitConfig);
                var firstPublisher = new RabbitPublisherFactory<TestMessageModel>(publisherConnectionManager, Configuration.TestExchange).Create();
                var secondPublisher = new RabbitPublisherFactory<TestMessageModel>(publisherConnectionManager, Configuration.TestExchange).Create();

                firstPublisher.Publish(_publishedMessages[0], message => _failedMessages.Add(message));
                secondPublisher.Publish(_publishedMessages[1], message => _failedMessages.Add(message));

                // Give the system a little time to process the message (and possibly fail to route it)
                Thread.Sleep(1000);

                _receivedMessages.Add(testConsumer.TryGetMessage(TimeSpan.FromSeconds(1)));
                _receivedMessages.Add(testConsumer.TryGetMessage(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void ThenTheMessagesAreQueued()
            {
                AssertTestMessageEquality(_receivedMessages[0], _publishedMessages[0]);
                AssertTestMessageEquality(_receivedMessages[1], _publishedMessages[1]);
            }

            [Test]
            public void ThenNoMessagesFail()
            {
                Assert.That(_failedMessages.Count, Is.EqualTo(0));
            }
        }
    }
}
