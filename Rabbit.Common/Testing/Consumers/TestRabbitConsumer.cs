using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Rabbit.Common.Factories;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit.Common.Testing.Consumers
{
    public class TestRabbitConsumer<TMessage> : IDisposable
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

        public static TestRabbitConsumer<TMessage> CreateWithTempQueueAndStart(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules)
        {
            return CreateWithTempQueueAndStart(rabbitConfig, testExchange, routingRules, string.Empty);
        }

        public static TestRabbitConsumer<TMessage> CreateWithTempQueueAndStart(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules, string routingKey)
        {
            var consumer = new TestRabbitConsumer<TMessage>(rabbitConfig, testExchange, routingRules, routingKey);
            consumer.Start();
            return consumer;
        }

        public static TestRabbitConsumer<TMessage> CreateForExistingQueueAndStart(RabbitConfig rabbitConfig, string queueName)
        {
            var consumer = new TestRabbitConsumer<TMessage>(rabbitConfig, queueName);
            consumer.StartWithExistingQueue();
            return consumer;
        }

        private TestRabbitConsumer(RabbitConfig rabbitConfig, string testExchange, IDictionary<string, string> routingRules, string routingKey)
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

        private TestRabbitConsumer(RabbitConfig rabbitConfig, string existingQueueName)
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
            _connection = new RabbitConnectionFactory().CreateAndConnect(_rabbitConfig).Get();
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
            _connection = new RabbitConnectionFactory().CreateAndConnect(_rabbitConfig).Get();
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

        public IEnumerable<RabbitMessage<TMessage>> TryGetSeveralMessages(int messageCount, TimeSpan maxWaitTime)
        {
            var messages = new List<RabbitMessage<TMessage>>();

            var timer = new Stopwatch();
            timer.Start();

            while(messages.Count != messageCount && timer.ElapsedMilliseconds <= maxWaitTime.TotalMilliseconds)
            {
                var message = TryGetMessage(maxWaitTime);
                if (message != null)
                {
                    messages.Add(message);
                }
            }

            timer.Stop();

            return messages;
        }

        public RabbitMessage<TMessage> TryGetMessage(TimeSpan maxWaitTime)
        {
            if (_receivedMessages.Count == 0)
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
