using System;
using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Interfaces.Publishers;
using Rabbit.Common.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit.Common.Publishers
{
    public class RabbitQueuePublisher<TMessageBody> : IQueuePublisher<TMessageBody>
    {
        private readonly string _exchangeName;
        private readonly IRabbitHeaderEncoder _headerEncoder;
        private readonly IRabbitBodyEncoder<TMessageBody> _bodyEncoder;
        private readonly IConnection _connection;

        public RabbitQueuePublisher(IRabbitConnection connection, string exchangeName,
                                    IRabbitHeaderEncoder headerEncoder, IRabbitBodyEncoder<TMessageBody> bodyEncoder)
        {
            _exchangeName = exchangeName;
            _headerEncoder = headerEncoder;
            _bodyEncoder = bodyEncoder;
            _connection = connection.Get();
        }

        public void Publish(RabbitMessage<TMessageBody> rabbitMessage, Action<FailedRabbitMessage<TMessageBody>> onFailure = null)
        {
            using (var channel = _connection.CreateModel())
            {
                var basicProperties = channel.CreateBasicProperties();

                var encodedBody = _bodyEncoder.Encode(rabbitMessage.Body);
                basicProperties.Headers = _headerEncoder.Encode(rabbitMessage.Headers);

                if (onFailure != null)
                {
                    channel.BasicReturn += (sender, args) => onFailure(GetFailedMessage(sender, args));
                }

                channel.BasicPublish(_exchangeName, string.Empty, true, basicProperties, encodedBody);
            }
        }

        private FailedRabbitMessage<TMessageBody> GetFailedMessage(object sender, BasicReturnEventArgs basicReturnEventArgs)
        {
            var headers = _headerEncoder.Decode(basicReturnEventArgs.BasicProperties.Headers);
            var body = _bodyEncoder.Decode(basicReturnEventArgs.Body);
            var rabbitMessage = new RabbitMessage<TMessageBody>(headers, body);

            var replyCode = basicReturnEventArgs.ReplyCode;
            var replyText = basicReturnEventArgs.ReplyText;
            return new FailedRabbitMessage<TMessageBody>(rabbitMessage, replyCode, replyText);
        }
    }
}