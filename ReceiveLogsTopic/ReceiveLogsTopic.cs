using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace ReceiveLogsTopic
{
    class Program
    {
        // 基于多个条件进行路由
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("topic_logs", ExchangeType.Topic);

                var queueName = channel.QueueDeclare().QueueName;

                if (args.Length < 1)
                {
                    Console.Error.WriteLine($"Usage:{Environment.GetCommandLineArgs()[0]}");
                    Console.WriteLine("Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }

                foreach (var bindingKey in args)
                {
                    channel.QueueBind(queueName, "topic_logs", bindingKey);
                }

                Console.WriteLine("[*] waiting for messages");

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var routingKey = ea.RoutingKey;
                    Console.WriteLine($"[x] Received '{routingKey}':'{message}'");
                    Console.WriteLine("[x] Done");
                };

                channel.BasicConsume(queueName, true, consumer);

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
