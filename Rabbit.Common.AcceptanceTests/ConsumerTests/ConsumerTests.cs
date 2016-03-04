using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Rabbit.Common.Factories;
using Rabbit.Common.Models;
using Rabbit.Common.Testing.Publishers;
using Rabbit.Common.Testing.QueueSetup;

namespace Rabbit.Common.AcceptanceTests.ConsumerTests
{
    [TestFixture]
    public class ConsumerTests
    {
        public class TestMessageModel
        {
            public string SomeProperty { get; set; }
        }

        public static void AssertTestMessageEquality(RabbitMessage<TestMessageModel> actualMessage, RabbitMessage<TestMessageModel> expectedMessage)
        {
            CollectionAssert.AreEquivalent(expectedMessage.Headers, actualMessage.Headers);

            Assert.That(actualMessage.Body.SomeProperty, Is.EqualTo(expectedMessage.Body.SomeProperty));
        }

        [TestFixture]
        public class GivenAMessageOnTheQueue
        {
            private List<RabbitMessage<TestMessageModel>> _testQueueMessages;
            private Queue<RabbitMessage<TestMessageModel>> _dequeuedMessages;
            private string _testQueueName;

            [OneTimeSetUp]
            public void WhenListeningForRabbitMessages()
            {
                _testQueueName = "RabbitCommonConsumerTests." + Guid.NewGuid();
                var routingRules = new Dictionary<string, string> { { "testHeaderKey", "testHeaderValue" } };

                _testQueueMessages = new List<RabbitMessage<TestMessageModel>>
                { 
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>(routingRules) { { "anotherKey", "withAnotherValue" } },
                        new TestMessageModel { SomeProperty = "Hello world" }
                    ),
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>(routingRules) { { "anotherKey", "withAnotherOtherValue" } },
                        new TestMessageModel { SomeProperty = "Wdwdw woodwood" }
                    )
                };

                _dequeuedMessages = new Queue<RabbitMessage<TestMessageModel>>();

                TestQueueCreator.CreateHeaderExchangeQueue(Configuration.RabbitConfig, Configuration.TestExchange, _testQueueName, routingRules);
                TestQueuePublisher.PublishMany(Configuration.RabbitConfig, Configuration.TestExchange, _testQueueMessages);

                var connection = new RabbitConnectionFactory().Create(Configuration.RabbitConfig);
                connection.Connect();

                var consumerFactory = new BlockingRabbitConsumerFactory<TestMessageModel>(connection);

                var consumingTask = new Task(() =>
                {
                    var consumer = consumerFactory.CreateQueueConsumer(_testQueueName);

                    _dequeuedMessages.Enqueue(consumer.Dequeue());
                    _dequeuedMessages.Enqueue(consumer.Dequeue());
                });
                consumingTask.Start();

                Task.WaitAll(new[] { consumingTask }, TimeSpan.FromSeconds(100000));
            }

            [Test]
            public void ThenTheMessagesAreDequeued()
            {
                Assert.That(_dequeuedMessages.Count, Is.EqualTo(2));
                AssertTestMessageEquality(_dequeuedMessages.Dequeue(), _testQueueMessages[0]);
                AssertTestMessageEquality(_dequeuedMessages.Dequeue(), _testQueueMessages[1]);
            }

            [OneTimeTearDown]
            public void TearItDown()
            {
                TestQueueRemover.RemoveQueue(Configuration.RabbitConfig, _testQueueName);
            }
        }
    }
}
