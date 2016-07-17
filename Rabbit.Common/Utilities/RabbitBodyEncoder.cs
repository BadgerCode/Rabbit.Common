using System.Text;
using Newtonsoft.Json;

namespace Rabbit.Common.Utilities
{
    public interface IRabbitBodyEncoder<T>
    {
        byte[] Encode(T body);
        T Decode(byte[] body);
    }

    public class RabbitBodyEncoder<T> : IRabbitBodyEncoder<T>
    {
        public byte[] Encode(T body)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
        }

        public T Decode(byte[] body)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));
        }
    }
}