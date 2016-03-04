﻿using Rabbit.Common.Interfaces.Connection;
using Rabbit.Common.Interfaces.Factories;
using Rabbit.Common.Interfaces.Models;
using Rabbit.Common.Interfaces.Publishers;
using Rabbit.Common.Publishers;
using Rabbit.Common.Utilities;

namespace Rabbit.Common.Factories
{
    public class RabbitPublisherFactory<TMessageBody> : IQueuePublisherFactory<RabbitMessage<TMessageBody>, FailedRabbitMessage<TMessageBody>>
    {
        private readonly IRabbitConnection _connection;
        private readonly string _exchange;

        public RabbitPublisherFactory(IRabbitConnection connection, string exchange)
        {
            _connection = connection;
            _exchange = exchange;
        }

        public IQueuePublisher<RabbitMessage<TMessageBody>, FailedRabbitMessage<TMessageBody>> Create()
        {
            return new RabbitQueuePublisher<TMessageBody>(_connection, _exchange, new RabbitHeaderEncoder(), new RabbitBodyEncoder<TMessageBody>());
        }
    }
}