namespace Rabbit.Common.Interfaces.Models
{
    public class RabbitConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string OriginatingHost { get; set; }
        public string ServiceName { get; set; }

        public RabbitConfig()
        {
            
        }

        public RabbitConfig(string username, string password, string host, int port, string virtualHost, string originatingHost, string serviceName)
        {
            Username = username;
            Password = password;
            Host = host;
            Port = port;
            VirtualHost = virtualHost;
            OriginatingHost = originatingHost;
            ServiceName = serviceName;
        }
    }
}