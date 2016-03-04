using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rabbit.Common.Connection;
using Rabbit.Common.Models;
using Rabbit.Common.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit.Common.Testing.Consumers
{
    public class TestMessageConsumer<TMessage> : IDisposable
    {
        private readonly RabbitConfig _rabbitConfig;
        private readonly string _existingQueueName;
        private readonly string _testExchange;
        private readonly IDictionary<string, string> _routingRules;
        private readonly string _routingKey;
        private readonly Queue<RabbitMessage<TMessage>> _receivedMessages;
        private IConnection _connection;
        private IModel _channel;
        private AutoResetEvent _waitHandle;
        private readonly RabbitHeaderEncoder _headerEncoder;
        private readonly RabbitBodyEncoder<TMessage> _bodyEncoder;

        public static TestMessageConsumer<TMessage> CreateWithTempQueueAndStart(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules)
        {
            return CreateWithTempQueueAndStart(rabbitConfig, testExchange, routingRules, string.Empty);
        }

        public static TestMessageConsumer<TMessage> CreateWithTempQueueAndStart(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules, string routingKey)
        {
            var consumer = new TestMessageConsumer<TMessage>(rabbitConfig, testExchange, routingRules, routingKey);
            consumer.Start();
            return consumer;
        }

        public static TestMessageConsumer<TMessage> CreateForExistingQueueAndStart(RabbitConfig rabbitConfig, string queueName)
        {
            var consumer = new TestMessageConsumer<TMessage>(rabbitConfig, queueName);
            consumer.StartWithExistingQueue();
            return consumer;
        }

        private TestMessageConsumer(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules, string routingKey)
        {
            _rabbitConfig = rabbitConfig;
            _testExchange = testExchange;
            _routingRules = routingRules;
            _routingKey = routingKey;

            _waitHandle = new AutoResetEvent(false);
            _receivedMessages = new Queue<RabbitMessage<TMessage>>();
            _headerEncoder = new RabbitHeaderEncoder();
            _bodyEncoder = new RabbitBodyEncoder<TMessage>();
        }

        private TestMessageConsumer(RabbitConfig rabbitConfig, string existingQueueName)
        {
            _rabbitConfig = rabbitConfig;
            _existingQueueName = existingQueueName;

            _waitHandle = new AutoResetEvent(false);
            _receivedMessages = new Queue<RabbitMessage<TMessage>>();
            _headerEncoder = new RabbitHeaderEncoder();
            _bodyEncoder = new RabbitBodyEncoder<TMessage>();
        }

        private void Start()
        {
            _connection = new RabbitConnectionFactory().Create(_rabbitConfig).Get();
            _channel = _connection.CreateModel();

            var tempQueueName = _channel.QueueDeclare().QueueName;
            var headerRouting = _headerEncoder.Encode(_routingRules);
            _channel.QueueBind(tempQueueName, _testExchange, _routingKey, headerRouting);
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ProcessReceivedMessage;

            _channel.BasicConsume(tempQueueName, true, consumer);
        }

        private void StartWithExistingQueue()
        {
            _connection = new RabbitConnectionFactory().Create(_rabbitConfig).Get();
            _channel = _connection.CreateModel();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ProcessReceivedMessage;

            _channel.BasicConsume(_existingQueueName, true, consumer);
        }

        private void ProcessReceivedMessage(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var headers = _headerEncoder.Decode(basicDeliverEventArgs.BasicProperties.Headers);
            var body = _bodyEncoder.Decode(basicDeliverEventArgs.Body);
            _receivedMessages.Enqueue(new RabbitMessage<TMessage>(headers, body));

            _waitHandle.Set();
        }

        public RabbitMessage<TMessage> TryGetMessage(TimeSpan maxWaitTime)
        {
            if (!_receivedMessages.Any())
            {
                var commandReceived = _waitHandle.WaitOne(maxWaitTime);

                if (!commandReceived)
                {
                    return null;
                }

                _waitHandle.Reset();
            }

            return _receivedMessages.Dequeue();
        }

        public void Dispose()
        {
            _connection.Abort();
        }
    }
}
