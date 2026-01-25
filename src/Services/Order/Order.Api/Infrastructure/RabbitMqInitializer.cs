 using RabbitMQ.Client;
using System;

namespace Order.Api.Infrastructure
{
    public static class RabbitMqInitializer
    {
        public static void Initialize(IConfiguration configuration)
        {
            IConfigurationSection rabbitConfig = configuration.GetSection("MessageBroker:RabbitMq");
            string? hostName = rabbitConfig["HostName"];
            int port = int.Parse(rabbitConfig["Port"] ?? "5672");
            string? userName = rabbitConfig["UserName"];
            string? password = rabbitConfig["Password"];
            string? exchangeName = rabbitConfig["ExchangeName"];
            string exchangeType = rabbitConfig["ExchangeType"] ?? "topic";
            string queuePrefix = rabbitConfig["QueuePrefix"] ?? "order-";
            string queueName = $"{queuePrefix}queue";

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            const int maxRetries = 5;
            int retryCount = 0;
            int delayMs = 2000;
            Exception lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    using IConnection? connection = factory.CreateConnection();
                    using IModel? channel = connection.CreateModel();

                    // Declara a exchange
                    channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);

                    // Declara a queue
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                    // Faz o binding da queue com a exchange
                    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "#");

                    return; // Sucesso
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;
                    if (retryCount == maxRetries)
                    {
                        Console.WriteLine($"[RabbitMqInitializer] Falha ao conectar ao RabbitMQ após {maxRetries} tentativas: {ex.Message}");
                        throw;
                    }
                    Console.WriteLine($"[RabbitMqInitializer] Tentativa {retryCount} falhou: {ex.Message}. Retentando em {delayMs}ms...");
                    System.Threading.Thread.Sleep(delayMs);
                    delayMs *= 2; // Exponencial
                }
            }
            if (lastException != null)
                throw lastException;
        }
    }
}
