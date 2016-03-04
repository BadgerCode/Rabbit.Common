using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rabbit.Common.Utilities
{
    public interface IRabbitHeaderEncoder
    {
        IDictionary<string, object> Encode(IDictionary<string, string> headers);
        IDictionary<string, string> Decode(IDictionary<string, object> headers);

        KeyValuePair<string, object> Encode(KeyValuePair<string, string> header);
        KeyValuePair<string, string> Decode(KeyValuePair<string, object> header);
    }

    public class RabbitHeaderEncoder : IRabbitHeaderEncoder
    {
        public IDictionary<string, object> Encode(IDictionary<string, string> headers)
        {
            return headers.ToDictionary(header => header.Key, header => (object) Encoding.UTF8.GetBytes(header.Value));
        }

        public IDictionary<string, string> Decode(IDictionary<string, object> headers)
        {
            return headers.ToDictionary(header => header.Key, header => Encoding.UTF8.GetString((byte[]) header.Value));
        }

        public KeyValuePair<string, object> Encode(KeyValuePair<string, string> header)
        {
            return new KeyValuePair<string, object>(header.Key, Encoding.UTF8.GetBytes(header.Value));
        }

        public KeyValuePair<string, string> Decode(KeyValuePair<string, object> header)
        {
            return new KeyValuePair<string, string>(header.Key, Encoding.UTF8.GetString((byte[]) header.Value));
        }
    }
}