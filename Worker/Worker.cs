using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("task_queue", true, false, false, null);

                // 公平调度 prefetchCount = 1 告诉RabbitMQ一次不要给同一个worker提供多于一条的信息
                channel.BasicQos(0, prefetchCount: 1, false);

                Console.WriteLine("[*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[x] Received {message}");

                    int dots = message.Split(".").Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine("[x] Done");

                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume("task_queue", false, consumer);

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
