using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Collections.Concurrent;

namespace RPCClient
{
    public class RPC
    {
        static void Main(string[] args)
        {
            var rpcClient = new RPCClient();
            Console.WriteLine("[x] Requesting fib(30)");
            var response = rpcClient.Call("30");

            Console.WriteLine($"[.] Got {response}");
            rpcClient.Close();
            Console.ReadLine();
        }
    }

    public class RPCClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> resQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        public RPCClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    resQueue.Add(response);
                }
            };
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", "rpc_queue", props, messageBytes);

            channel.BasicConsume(consumer, replyQueueName, true);

            return resQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
