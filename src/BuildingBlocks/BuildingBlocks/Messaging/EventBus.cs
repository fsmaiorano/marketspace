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
using RabbitMQ.Client.Exceptions;
using System.Reflection;
using Serilog.Context;

namespace BuildingBlocks.Messaging;

/// <summary>
/// RabbitMQ implementation of event bus for distributed event-driven communication.
/// 
/// Exchange topology:
///   - One fanout exchange per integration event type: marketspace.{EventTypeName}
///   - One dedicated fanout dead letter exchange per subscriber: marketspace.{HandlerName}_dlx
///
/// Each subscriber gets its own consumer channel, so a channel exception on one
/// subscription does not affect publishing or other subscriptions.
/// </summary>
public class EventBus : IEventBus, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _publishChannel;
    private readonly List<IChannel> _consumerChannels = new();
    private readonly Dictionary<Type, List<Type>> _handlers = new();
    private readonly HashSet<string> _declaredExchanges = new();
    private readonly object _exchangeDeclareLock = new();
    private const string ExchangePrefix = "marketspace";

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
                _publishChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

                _logger.LogInformation("RabbitMQ EventBus initialized");
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

    /// <summary>Returns the fanout exchange name for a given integration event type.</summary>
    private static string GetExchangeName(Type eventType) => $"{ExchangePrefix}.{eventType.Name}";

    /// <summary>Returns the dedicated dead letter exchange name for a subscriber handler.</summary>
    private static string GetDeadLetterExchangeName(string handlerName) => $"{ExchangePrefix}.{handlerName}_dlx";

    /// <summary>
    /// Declares a fanout exchange on the publish channel, skipping if already declared.
    /// </summary>
    private async Task EnsurePublishExchangeDeclaredAsync(string exchangeName)
    {
        // Fast path check under lock to avoid redundant declares from concurrent publishers
        bool alreadyDeclared;
        lock (_exchangeDeclareLock)
        {
            alreadyDeclared = _declaredExchanges.Contains(exchangeName);
        }

        if (alreadyDeclared)
            return;

        await _publishChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        lock (_exchangeDeclareLock)
        {
            _declaredExchanges.Add(exchangeName);
        }

        _logger.LogInformation("Declared exchange {ExchangeName}", exchangeName);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        Type eventType = @event.GetType();
        string exchangeName = GetExchangeName(eventType);

        _logger.LogInformation("Publishing integration event {EventType} with EventId {EventId} to exchange {ExchangeName}",
            eventType.Name, @event.EventId, exchangeName);

        await EnsurePublishExchangeDeclaredAsync(exchangeName);

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

        // Fanout exchanges route to all bound queues; routing key is intentionally empty
        await _publishChannel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Published event {EventType} to exchange {ExchangeName}", eventType.Name, exchangeName);
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

        string safeHandlerName = (handlerType.FullName ?? handlerType.Name)
            .Replace('+', '.')
            .Replace('<', '_')
            .Replace('>', '_');
        string queueName = safeHandlerName;
        string deadLetterQueueName = $"{safeHandlerName}_dlq";
        string exchangeName = GetExchangeName(eventType);
        string deadLetterExchangeName = GetDeadLetterExchangeName(safeHandlerName);

        // Each subscription uses a dedicated channel so a channel-level exception
        // (e.g. PRECONDITION_FAILED on queue redeclaration) does not affect publishing
        // or other subscriptions.
        IChannel channel = await _connection.CreateChannelAsync();
        _consumerChannels.Add(channel);

        // Declare the per-event fanout exchange (idempotent)
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        // Declare the per-subscriber dedicated dead letter exchange (idempotent)
        await channel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        // Declare and bind the dead letter queue (QueueDeclare is idempotent when args match)
        await channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: string.Empty);

        // Declare the main queue pointing to its dedicated DLX.
        // If the queue already exists with different arguments (e.g. from a previous
        // topology), recover: open a new channel, delete the stale queue and recreate it.
        Dictionary<string, object?> queueArguments = new() { { "x-dead-letter-exchange", deadLetterExchangeName } };

        channel = await DeclareMainQueueAsync(channel, queueName, queueArguments,
            exchangeName, deadLetterExchangeName, deadLetterQueueName);

        // Bind the main queue to the per-event fanout exchange (routing key unused in fanout)
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty);

        // Set up consumer
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
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
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        IIdempotencyService? idempotencyService = scope.ServiceProvider.GetService<IIdempotencyService>();

                        if (!string.IsNullOrWhiteSpace(correlationId))
                        {
                            ICorrelationIdService? correlationIdService =
                                scope.ServiceProvider.GetService<ICorrelationIdService>();
                            correlationIdService?.SetCorrelationId(correlationId);
                        }

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

                    await channel.BasicAckAsync(deliveryTag, false);
                    _logger.LogDebug("Message acknowledged for event {EventType}", eventType.Name);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to deserialize event {EventType}. Rejecting message to dead letter queue.",
                        eventType.Name);

                    await channel.BasicRejectAsync(deliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}. Rejecting message to dead letter queue.",
                    eventType.Name);

                await channel.BasicRejectAsync(deliveryTag, false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Started consuming queue {QueueName} bound to exchange {ExchangeName}. Dead letters → {DeadLetterExchangeName} → {DeadLetterQueueName}",
            queueName, exchangeName, deadLetterExchangeName, deadLetterQueueName);
    }

    /// <summary>
    /// Declares the main consumer queue. If the queue already exists with different arguments
    /// (PRECONDITION_FAILED), recovers by opening a new channel, deleting the stale queue, and
    /// recreating it with the correct arguments.
    /// </summary>
    private async Task<IChannel> DeclareMainQueueAsync(
        IChannel channel,
        string queueName,
        Dictionary<string, object?> queueArguments,
        string exchangeName,
        string deadLetterExchangeName,
        string deadLetterQueueName)
    {
        try
        {
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);

            return channel;
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 406)
        {
            // PRECONDITION_FAILED: queue exists with different arguments (stale topology).
            // The channel is now closed; open a fresh one, delete the stale queue and recreate.
            _logger.LogWarning(
                "Queue {QueueName} exists with incompatible arguments. Recreating with updated topology...",
                queueName);

            IChannel recoveryChannel = await _connection.CreateChannelAsync();
            _consumerChannels[^1] = recoveryChannel;

            // Re-declare exchanges on the new channel (idempotent)
            await recoveryChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
            await recoveryChannel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

            // Re-declare DLQ binding on the new channel
            await recoveryChannel.QueueDeclareAsync(deadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
            await recoveryChannel.QueueBindAsync(deadLetterQueueName, deadLetterExchangeName, routingKey: string.Empty);

            // Delete stale queue and recreate with correct arguments
            await recoveryChannel.QueueDeleteAsync(queueName, ifUnused: false, ifEmpty: false);
            await recoveryChannel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);

            _logger.LogInformation("Queue {QueueName} successfully recreated with updated arguments", queueName);
            return recoveryChannel;
        }
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

        foreach (IChannel consumerChannel in _consumerChannels)
        {
            await consumerChannel.CloseAsync();
            consumerChannel.Dispose();
        }

        await _publishChannel.CloseAsync();
        await _connection.CloseAsync();
        _publishChannel.Dispose();
        _connection.Dispose();
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ EventBus");

        foreach (IChannel consumerChannel in _consumerChannels)
            consumerChannel.Dispose();

        _publishChannel.Dispose();
        _connection.Dispose();
    }
}
