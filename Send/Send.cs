using System;
using RabbitMQ.Client;
using System.Text;
namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("hello", false, false, false, null);

                string message = "Hello world";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("", "hello", null, body);
                Console.WriteLine($"[x] Sent {message}");
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
