using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            // BasicReceive(args);
            // ConfirmReceive(args);
            // FairReceive(args);
            Fanout(args);

            Console.ReadKey();
        }

        /// <summary>
        /// 基础接收
        /// </summary>
        /// <param name="args"></param>
        static void BasicReceive(string[] args)
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

                    // 5.构造消费者实例
                    var consumer = new EventingBasicConsumer(channel);
                    // 6.绑定消息接收后的事件委托
                    consumer.Received += (model, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        Console.WriteLine(" [x] Received {0}", message);
                        Thread.Sleep(6000);  // 模拟耗时
                        Console.WriteLine(" [x] Done");
                    };

                    // 7.启动消费者
                    channel.BasicConsume("hello", true, consumer);
                    Console.WriteLine(" Press [enter] to exit.");
                }
            }
        }

        /// <summary>
        /// 消息确认模式
        /// </summary>
        /// <param name="args"></param>
        static void ConfirmReceive(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory();
            // 2.建立连接
            var connection = factory.CreateConnection();

            // 3.创建管道
            var channel = connection.CreateModel();

            // 4.申明队列
            channel.QueueDeclare("hello", true, false, false, null);

            // 5.构造消费者实例
            var consumer = new EventingBasicConsumer(channel);
            // 6.绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine(" [x] Received {0}", message);
                Thread.Sleep(6000);  // 模拟耗时
                Console.WriteLine(" [x] Done");

                // 7.发送消息确认信号（手动消息确认）                      
                channel.BasicAck(ea.DeliveryTag, false);
            };

            // 8.启动消费者
            channel.BasicConsume("hello", false, consumer);

            Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadKey();
            //channel.Dispose();
            //connection.Dispose();
        }

        /// <summary>
        /// 公平分发
        /// </summary>
        /// <param name="args"></param>
        static void FairReceive(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory();
            // 2.建立连接
            var connection = factory.CreateConnection();

            // 3.创建管道
            var channel = connection.CreateModel();

            // 4.申明队列
            channel.QueueDeclare("hello", true, false, false, null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // 5.构造消费者实例
            var consumer = new EventingBasicConsumer(channel);
            // 6.绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine(" [x] Received {0}", message);
                Thread.Sleep(6000);  // 模拟耗时
                Console.WriteLine(" [x] Done");

                // 7.发送消息确认信号（手动消息确认）                      
                channel.BasicAck(ea.DeliveryTag, false);
            };

            // 8.启动消费者
            channel.BasicConsume("hello", false, consumer);

            Console.WriteLine(" Press [enter] to exit.");
        }

        static void Fanout(string[] args)
        {
            // 1.实例化连接工厂
            var factory = new ConnectionFactory();
            // 2.建立连接
            var connection = factory.CreateConnection();

            // 3.创建管道
            var channel = connection.CreateModel();

            // 4.申明fanout类型exchange
            channel.ExchangeDeclare("fanoutEC", "fanout");
            // 申明随机队列名称
            var queueName = channel.QueueDeclare().QueueName;

            // 绑定队列到指定fannout类型exchange，无需指定路由键
            channel.QueueBind(queueName, "fanoutEC", "");

            // 5.构造消费者实例
            var consumer = new EventingBasicConsumer(channel);
            // 6.绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine(" [x] Received {0}", message);
                Thread.Sleep(6000);  // 模拟耗时
                Console.WriteLine(" [x] Done");

                // 7.发送消息确认信号（手动消息确认）                      
                channel.BasicAck(ea.DeliveryTag, false);
            };

            // 8.启动消费者
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine(" Press [enter] to exit.");
        }

    }
}
