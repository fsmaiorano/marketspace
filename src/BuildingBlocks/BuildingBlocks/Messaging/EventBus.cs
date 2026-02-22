using System.Text;
using System.Text.Json;
using BuildingBlocks.Messaging.Idempotency;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using BuildingBlocks.Services.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using Serilog.Context;

namespace BuildingBlocks.Messaging;

/// <summary>
/// RabbitMQ implementation of event bus for distributed event-driven communication
/// </summary>
public class EventBus : IEventBus, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly Dictionary<Type, List<Type>> _handlers = new();
    private readonly string _exchangeName = "marketspace_events";

    public EventBus(IServiceProvider serviceProvider, ILogger<EventBus> logger, string rabbitMqConnectionString)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        ConnectionFactory factory = new() { Uri = new Uri(rabbitMqConnectionString) };

        int attempt = 0;
        TimeSpan delay = TimeSpan.FromSeconds(2);

        while (true)
        {
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, durable: true, autoDelete: false)
                    .GetAwaiter().GetResult();

                _logger.LogInformation("RabbitMQ EventBus initialized with exchange {ExchangeName}", _exchangeName);
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Unable to connect to RabbitMQ (attempt {Attempt}). Retrying in {Delay}s...",
                    attempt, delay.TotalSeconds);

                Thread.Sleep(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        Type eventType = @event.GetType();
        string routingKey = eventType.Name;

        _logger.LogInformation("Publishing integration event {EventType} with EventId {EventId}",
            eventType.Name, @event.EventId);

        string message = JsonSerializer.Serialize(@event,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        byte[] body = Encoding.UTF8.GetBytes(message);

        BasicProperties properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.EventId.ToString(),
            Timestamp = new AmqpTimestamp(new DateTimeOffset(@event.OccurredAt).ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>()
        };

        // Propagate CorrelationId via headers for distributed tracing
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            ICorrelationIdService? correlationIdService = scope.ServiceProvider.GetService<ICorrelationIdService>();
            if (correlationIdService != null)
            {
                string correlationId = correlationIdService.GetCorrelationId();
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    properties.Headers["X-Correlation-ID"] = correlationId;
                    _logger.LogDebug("Publishing event with CorrelationId: {CorrelationId}", correlationId);
                }
            }
        }

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Published event {EventType} to RabbitMQ with routing key {RoutingKey}",
            eventType.Name, routingKey);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        SubscribeAsync<TEvent, THandler>().GetAwaiter().GetResult();
    }

    private async Task SubscribeAsync<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        Type eventType = typeof(TEvent);
        Type handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Type>();

        if (!_handlers[eventType].Contains(handlerType))
        {
            _handlers[eventType].Add(handlerType);
            _logger.LogInformation("Subscribed {HandlerType} to {EventType}", handlerType.Name, eventType.Name);
        }

        // Create dead letter queue and exchange
        string queueName = $"{eventType.Name}";
        string deadLetterQueueName = $"{eventType.Name}_dlq";
        string deadLetterExchangeName = $"{_exchangeName}_dlq";
        string routingKey = eventType.Name;

        // Declare dead letter exchange
        await _channel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Topic, durable: true,
            autoDelete: false);

        // Declare dead letter queue
        await _channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Bind dead letter queue to dead letter exchange
        await _channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: routingKey);

        // Create main queue with dead letter configuration
        Dictionary<string, object?> queueArguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", deadLetterExchangeName }, { "x-dead-letter-routing-key", routingKey }
        };

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _exchangeName,
            routingKey: routingKey);

        // Set up consumer
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            ulong deliveryTag = ea.DeliveryTag;

            try
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                // Extract CorrelationId from message headers
                string? correlationId = null;
                if (ea.BasicProperties?.Headers?.TryGetValue("X-Correlation-ID", out object? correlationIdObj) == true)
                {
                    correlationId = correlationIdObj switch
                    {
                        byte[] bytes => Encoding.UTF8.GetString(bytes),
                        string str => str,
                        _ => correlationIdObj?.ToString()
                    };
                }

                _logger.LogDebug("Received event {EventType} from RabbitMQ with CorrelationId: {CorrelationId}",
                    eventType.Name, correlationId ?? "N/A");

                _logger.LogDebug("Raw message content: {Message}", message);

                TEvent? @event = JsonSerializer.Deserialize<TEvent>(message,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
                    });

                if (@event != null)
                {
                    // Set CorrelationId in scope before processing
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        IIdempotencyService? idempotencyService = scope.ServiceProvider.GetService<IIdempotencyService>();

                        if (!string.IsNullOrWhiteSpace(correlationId))
                        {
                            ICorrelationIdService? correlationIdService =
                                scope.ServiceProvider.GetService<ICorrelationIdService>();
                            correlationIdService?.SetCorrelationId(correlationId);
                        }

                        // Add to Serilog context for structured logging
                        using (LogContext.PushProperty("CorrelationId", correlationId ?? "N/A"))
                        {
                            if (idempotencyService != null)
                            {
                                await idempotencyService.ExecuteAsync(@event.EventId, eventType.Name, async (ct) =>
                                {
                                    await ProcessEventAsync(@event, eventType, scope.ServiceProvider, ct);
                                }, CancellationToken.None);
                            }
                            else
                            {
                                await ProcessEventAsync(@event, eventType, scope.ServiceProvider,
                                    CancellationToken.None);
                            }
                        }
                    }

                    // Acknowledge the message successfully processed
                    await _channel.BasicAckAsync(deliveryTag, false);
                    _logger.LogDebug("Message acknowledged for event {EventType}", eventType.Name);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to deserialize event {EventType}. Rejecting message to dead letter queue.",
                        eventType.Name);

                    // Reject message without requeue - will go to dead letter queue
                    await _channel.BasicRejectAsync(deliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}. Rejecting message to dead letter queue.",
                    eventType.Name);

                // Reject message without requeue - will go to dead letter queue
                await _channel.BasicRejectAsync(deliveryTag, false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Started consuming messages from queue {QueueName} with dead letter queue {DeadLetterQueueName}",
            queueName, deadLetterQueueName);
    }

    private async Task ProcessEventAsync<TEvent>(TEvent @event, Type eventType, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TEvent : class, IIntegrationEvent
    {
        if (!_handlers.TryGetValue(eventType, out List<Type>? handlerTypes))
        {
            _logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
            return;
        }

        List<Task> tasks = new List<Task>();

        foreach (Type handlerType in handlerTypes)
        {
            object? handler = serviceProvider.GetService(handlerType);
            if (handler is null)
            {
                _logger.LogWarning("Handler {HandlerType} not found in service provider", handlerType.Name);
                continue;
            }

            MethodInfo? method = handlerType.GetMethod("HandleAsync", new[] { eventType, typeof(CancellationToken) });
            Task? task = (Task?)method?.Invoke(handler, new object[] { @event, cancellationToken });

            if (task != null)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing RabbitMQ EventBus");
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        _channel.Dispose();
        _connection.Dispose();
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ EventBus");
        _channel.Dispose();
        _connection.Dispose();
    }
}