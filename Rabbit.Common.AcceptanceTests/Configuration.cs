using System.Configuration;
using Rabbit.Common.Interfaces.Models;

namespace Rabbit.Common.AcceptanceTests
{
    public static class Configuration
    {
        public static RabbitConfig RabbitConfig { get; set; }
        public static string TestExchange { get; set; }

        static Configuration()
        {
            RabbitConfig = new RabbitConfig
            {
                Host = ConfigurationManager.AppSettings["Rabbit.Hostname"],
                Port = int.Parse(ConfigurationManager.AppSettings["Rabbit.Port"]),
                Username = ConfigurationManager.AppSettings["Rabbit.UserName"],
                Password = ConfigurationManager.AppSettings["Rabbit.Password"],
                VirtualHost = ConfigurationManager.AppSettings["Rabbit.VirtualHost"],
                OriginatingHost = System.Environment.MachineName,
                ServiceName = ConfigurationManager.AppSettings["Rabbit.ServiceName"]
            };

            TestExchange = ConfigurationManager.AppSettings["Rabbit.ExchangeName"];
        }
    }
}
