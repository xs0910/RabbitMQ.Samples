using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace RPCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("rpc_queue", false, false, false, null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume("rpc_queue", false, consumer);

                Console.WriteLine("[x] Awaiting RPC requests");
                consumer.Received += (model, ea) =>
                {
                    string response = null;
                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int n = int.Parse(message);
                        Console.WriteLine($"[.] Fib({n})");
                        response = Fib(n).ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[.] " + ex.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                };

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }

        }

        //模拟耗时
        private static int Fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }
            return Fib(n - 1) + Fib(n - 2);
        }

    }
}
