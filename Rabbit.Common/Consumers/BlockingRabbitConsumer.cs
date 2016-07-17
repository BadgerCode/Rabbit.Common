using System;
using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Consumers;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit.Common.Consumers
{
    public class BlockingRabbitConsumer<TMessageBody> : IBlockingRabbitConsumer<TMessageBody>
    {
        private readonly IRabbitConnection _rabbitConnection;
        private readonly QueueingBasicConsumer _queueingBasicConsumer;
        private readonly IRabbitHeaderEncoder _headerEncoder;
        private readonly IRabbitBodyEncoder<TMessageBody> _bodyEncoder;

        public BlockingRabbitConsumer(IRabbitConnection rabbitConnection, QueueingBasicConsumer queueingBasicConsumer, 
                                      IRabbitHeaderEncoder headerEncoder, IRabbitBodyEncoder<TMessageBody> bodyEncoder)
        {
            _rabbitConnection = rabbitConnection;
            _queueingBasicConsumer = queueingBasicConsumer;
            _headerEncoder = headerEncoder;
            _bodyEncoder = bodyEncoder;
        }

        public RabbitMessage<TMessageBody> Dequeue()
        {
            BasicDeliverEventArgs queueMessage;

            try
            {
                queueMessage = _queueingBasicConsumer.Queue.Dequeue();
            }
            catch (Exception)
            {
                _rabbitConnection.Abort();
                throw;
            }

            var headers = _headerEncoder.Decode(queueMessage.BasicProperties.Headers);
            var body = _bodyEncoder.Decode(queueMessage.Body);

            return new RabbitMessage<TMessageBody>(headers, body);
        }
    }
}