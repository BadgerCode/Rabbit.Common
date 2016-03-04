using System.Collections.Generic;

namespace Rabbit.Common.Models
{
    public class RabbitMessage<TMessageBody>
    {
        public IDictionary<string, string> Headers { get; }

        public TMessageBody Body { get; }

        public RabbitMessage(IDictionary<string, string> headers, TMessageBody body)
        {
            Headers = headers;
            Body = body;
        }
    }
}