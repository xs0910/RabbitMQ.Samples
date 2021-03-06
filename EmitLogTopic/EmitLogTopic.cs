using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;

namespace EmitLogTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("topic_logs", ExchangeType.Topic);

                var routingKey = args.Length > 0 ? args[0] : "anonymous.info";
                var message = args.Length > 1 ? string.Join(" ", args.Skip(1).ToArray()) : "Hello world!";

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("topic_logs", routingKey, null, body);
                Console.WriteLine($"[x] sent '{routingKey}:'{message}'");                
            }
            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
