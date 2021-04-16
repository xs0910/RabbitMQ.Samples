using RabbitMQ.Client;
using System;
using System.Text;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            // BasicSend(args);
            // DurableSend(args);
            Fanout(args);

            Console.ReadKey();
        }

        /// <summary>
        /// 基础的发送
        /// </summary>
        /// <param name="args"></param>
        static void BasicSend(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // 2.建立连接
            using (var connection = factory.CreateConnection())
            {
                // 3.创建管道
                using (var channel = connection.CreateModel())
                {
                    // 4.申明队列
                    channel.QueueDeclare("hello", false, false, false, null);
                    // 5.构建byte消息数据包
                    string message = args.Length > 0 ? args[0] : "Hello RabbitMQ";
                    var body = Encoding.UTF8.GetBytes(message);
                    // 6.发送数据包
                    channel.BasicPublish("", "hello", null, body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }

        static void DurableSend(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // 2.建立连接
            using (var connection = factory.CreateConnection())
            {
                // 3.创建管道
                using (var channel = connection.CreateModel())
                {
                    // 4.申明队列
                    channel.QueueDeclare("hello", true, false, false, null);

                    // 将消息标记为持久化
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    // 5.构建byte消息数据包
                    string message = args.Length > 0 ? args[0] : "Hello RabbitMQ";
                    var body = Encoding.UTF8.GetBytes(message);

                    // 6.发送数据包（指定basicProperties)
                    channel.BasicPublish("", "hello", properties, body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }

        static void Fanout(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // 2.建立连接
            using var connection = factory.CreateConnection();
            // 3.创建管道
            using var channel = connection.CreateModel();
            // 4.使用fanout
            var queueName = channel.QueueDeclare().QueueName;
            channel.ExchangeDeclare("fanoutEC", "fanout");

            // 将消息标记为持久化
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // 5.构建byte消息数据包
            string message = args.Length > 0 ? args[0] : "Hello RabbitMQ";
            var body = Encoding.UTF8.GetBytes(message);

            // 6.发送数据包（指定basicProperties)
            channel.BasicPublish("fanoutEC", "", properties, body);

            Console.WriteLine(" [x] Sent {0}", message);
        }


    }
}
