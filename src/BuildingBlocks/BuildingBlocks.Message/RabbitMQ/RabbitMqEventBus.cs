using System.Collections.Concurrent;
using BuildingBlocks.Message.Abstractions;
using BuildingBlocks.Message.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BuildingBlocks.Message.RabbitMQ;

public sealed class RabbitMqEventBus(
    IConnection connection,
    IEventSerializer serializer,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    ILogger<RabbitMqEventBus> logger) : IEventBus, IAsyncDisposable
{
    private readonly IConnection _connection = connection;
    private readonly IEventSerializer _serializer = serializer;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly RabbitMqOptions _options = options.Value.RabbitMq;
    private readonly ILogger<RabbitMqEventBus> _logger = logger;
    private readonly ConcurrentBag<IModel> _channels = new();
    private readonly ConcurrentBag<AsyncEventingBasicConsumer> _consumers = new();

    public Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        string routingKey = BuildRoutingKey(topic);

        using IModel channel = _connection.CreateModel();
        channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true);

        IBasicProperties properties = channel.CreateBasicProperties();
        properties.MessageId = @event.Id.ToString();
        properties.ContentType = "application/json";
        properties.Type = typeof(TEvent).Name;
        properties.Headers = new Dictionary<string, object>
        {
            ["type"] = typeof(TEvent).FullName ?? typeof(TEvent).Name,
            ["occurredOn"] = @event.OccurredOn.ToString("O")
        };

        byte[] body = System.Text.Encoding.UTF8.GetBytes(_serializer.Serialize(@event));

        _logger.LogInformation("Publishing event {EventType} to exchange {Exchange} with routing key {RoutingKey}", typeof(TEvent).Name, _options.ExchangeName, routingKey);

        channel.BasicPublish(_options.ExchangeName, routingKey, properties, body);

        return Task.CompletedTask;
    }

    public async Task SubscribeAsync<TEvent, THandler>(string topic, string subscription, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        string routingKey = BuildRoutingKey(topic);
        string queueName = BuildQueueName(subscription);

        IModel channel = _connection.CreateModel();
        channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true);
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queueName, _options.ExchangeName, routingKey);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                TEvent? deserialized = _serializer.Deserialize<TEvent>(message);

                using IServiceScope scope = _serviceProvider.CreateScope();
                THandler handler = scope.ServiceProvider.GetRequiredService<THandler>();

                _logger.LogInformation("Handling event {EventType} from queue {Queue}", typeof(TEvent).Name, queueName);
                await handler.HandleAsync(deserialized, CancellationToken.None);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventType} from queue {Queue}", typeof(TEvent).Name, queueName);
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queueName, false, consumer);
        _channels.Add(channel);
        _consumers.Add(consumer);

        _logger.LogInformation("Subscribed to queue {Queue} with routing key {RoutingKey}", queueName, routingKey);
    }

    public ValueTask DisposeAsync()
    {
        foreach (IModel channel in _channels)
        {
            channel.Close();
            channel.Dispose();
        }

        _connection.Close();
        _connection.Dispose();

        return ValueTask.CompletedTask;
    }

    private string BuildRoutingKey(string topic) => topic;

    private string BuildQueueName(string subscription) => string.IsNullOrWhiteSpace(_options.QueuePrefix)
        ? subscription
        : string.Concat(_options.QueuePrefix, subscription);
}
