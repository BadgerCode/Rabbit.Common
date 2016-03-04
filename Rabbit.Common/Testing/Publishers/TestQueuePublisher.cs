using System;
using System.Collections.Generic;
using Rabbit.Common.Factories;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Utilities;
using RabbitMQ.Client.Events;

namespace Rabbit.Common.Testing.Publishers
{
    public static class TestQueuePublisher
    {
        public static void Publish<TMessage>(RabbitConfig rabbitConfig, string exchangeName, RabbitMessage<TMessage> messages, 
                                             Action<FailedRabbitMessage<TMessage>> onFailure = null)
        {
            PublishMany(rabbitConfig, exchangeName, string.Empty, new List<RabbitMessage<TMessage>> { messages }, onFailure);
        }

        public static void Publish<TMessage>(RabbitConfig rabbitConfig, string exchangeName, string routingKey, RabbitMessage<TMessage> messages,
                                             Action<FailedRabbitMessage<TMessage>> onFailure = null)
        {
            PublishMany(rabbitConfig, exchangeName, routingKey, new List<RabbitMessage<TMessage>> { messages }, onFailure);
        }

        public static void PublishMany<TMessage>(RabbitConfig rabbitConfig, string exchangeName, IList<RabbitMessage<TMessage>> messages,
                                             Action<FailedRabbitMessage<TMessage>> onFailure = null)
        {
            PublishMany(rabbitConfig, exchangeName, string.Empty, messages, onFailure);
        }

        public static void PublishMany<TMessage>(RabbitConfig rabbitConfig, string exchangeName, string routingKey, IList<RabbitMessage<TMessage>> messages, 
                                             Action<FailedRabbitMessage<TMessage>> onFailure = null)
        {
            using (var channel = new RabbitConnectionFactory().CreateAndConnect(rabbitConfig).Get().CreateModel())
            {
                if (onFailure != null)
                {
                    channel.BasicReturn += (sender, args) => onFailure(GetFailedMessage<TMessage>(sender, args));
                }

                foreach (var message in messages)
                {
                    var basicProperties = channel.CreateBasicProperties();

                    var encodedBody = new RabbitBodyEncoder<TMessage>().Encode(message.Body);
                    basicProperties.Headers = new RabbitHeaderEncoder().Encode(message.Headers);

                    channel.BasicPublish(exchangeName, routingKey, true, basicProperties, encodedBody);
                }
            }
        }

        private static FailedRabbitMessage<TMessage> GetFailedMessage<TMessage>(object sender, BasicReturnEventArgs basicReturnEventArgs)
        {
            var headers = new RabbitHeaderEncoder().Decode(basicReturnEventArgs.BasicProperties.Headers);
            var body = new RabbitBodyEncoder<TMessage>().Decode(basicReturnEventArgs.Body);
            var rabbitMessage = new RabbitMessage<TMessage>(headers, body);

            var replyCode = basicReturnEventArgs.ReplyCode;
            var replyText = basicReturnEventArgs.ReplyText;
            return new FailedRabbitMessage<TMessage>(rabbitMessage, replyCode, replyText);
        }
    }
}
