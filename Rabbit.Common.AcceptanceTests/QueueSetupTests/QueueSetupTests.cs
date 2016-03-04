using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rabbit.Common.Connection;
using Rabbit.Common.Factories;
using Rabbit.Common.Models;
using Rabbit.Common.Testing.Consumers;
using Rabbit.Common.Testing.Publishers;
using Rabbit.Common.Testing.QueueSetup;

namespace Rabbit.Common.AcceptanceTests.QueueSetupTests
{
    [TestFixture]
    public class QueueSetupTests
    {
        public class TestMessageModel
        {
            public string A { get; set; }
            public int B { get; set; }
        }

        [TestFixture]
        public class GivenAQueueNameAndHeaderRoutingRulesWhichMustAllMatch
        {
            private List<RabbitMessage<TestMessageModel>> _publishedMessages;
            private List<RabbitMessage<TestMessageModel>> _receivedMessages;
            private List<FailedRabbitMessage<TestMessageModel>> _failedMessages;
            private string _testqueuename;

            [OneTimeSetUp]
            public void WhenCreatingTheQueue()
            {
                _testqueuename = "RabbitCommonQueueSetupTests" + Guid.NewGuid();

                var routingRules = new Dictionary<string, string>
                {
                    { "ARoutingHeaderKey", "Value" + Guid.NewGuid() },
                    { "BRoutingHeaderKey", "Value" + Guid.NewGuid() }
                };

                _publishedMessages = new List<RabbitMessage<TestMessageModel>>
                {
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>(routingRules) { { "AKey", "AValue" } },
                        new TestMessageModel { A = "Hello", B = 10 }
                    ),
                    new RabbitMessage<TestMessageModel>(
                        new Dictionary<string, string>(routingRules) { { "AKey", "AValue" } },
                        new TestMessageModel { A = "Goodbyte", B = 13 }
                    )
                };

                _publishedMessages[1].Headers["BRoutingHeaderKey"] = "WrongValue";

                var connection = new RabbitConnectionFactory().Create(Configuration.RabbitConfig);

                var queueSetup = new RabbitQueueSetupFactory(connection).Create();
                queueSetup.CreateHeaderExchangeQueue(Configuration.TestExchange, _testqueuename, routingRules, true);

                _receivedMessages = new List<RabbitMessage<TestMessageModel>>();
                _failedMessages = new List<FailedRabbitMessage<TestMessageModel>>();
                var failedMessageHandle = new AutoResetEvent(false);

                TestQueuePublisher.PublishMany(Configuration.RabbitConfig, Configuration.TestExchange, _publishedMessages,
                    failedMessage =>
                    {
                        _failedMessages.Add(failedMessage);
                        failedMessageHandle.Set();
                    });

                var consumer = TestMessageConsumer<TestMessageModel>.CreateForExistingQueueAndStart(Configuration.RabbitConfig, _testqueuename);
                var receivedMessage = consumer.TryGetMessage(TimeSpan.FromSeconds(50));
                if (receivedMessage != null)
                {
                    _receivedMessages.Add(receivedMessage);
                }

                failedMessageHandle.WaitOne(TimeSpan.FromSeconds(5));
            }

            [Test]
            public void ThenTheQueueReceivesTheFirstMessage()
            {
                Assert.That(_receivedMessages.Count, Is.EqualTo(1));

                var receivedMessage = _receivedMessages[0];
                Assert.That(receivedMessage.Headers["AKey"], Is.EqualTo(_publishedMessages[0].Headers["AKey"]));

                Assert.That(receivedMessage.Body.A, Is.EqualTo(_publishedMessages[0].Body.A));
                Assert.That(receivedMessage.Body.B, Is.EqualTo(_publishedMessages[0].Body.B));
            }

            [OneTimeTearDown]
            public void TearItDown()
            {
                TestQueueRemover.RemoveQueue(Configuration.RabbitConfig, _testqueuename);
            }
        }
    }
}
