using System;
using RabbitMQ.Client;
using System.Text;
namespace EmitLog
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("logs", ExchangeType.Fanout);

                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("logs", "", null, body);
                Console.WriteLine($"[x] send {message}");
            }
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");
        }
    }
}
