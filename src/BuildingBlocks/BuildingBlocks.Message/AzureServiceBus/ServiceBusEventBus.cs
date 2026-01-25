using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using BuildingBlocks.Message.Abstractions;
using BuildingBlocks.Message.Configuration;
using BuildingBlocks.Message.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Message.AzureServiceBus;

public sealed class ServiceBusEventBus(
    ServiceBusClient client,
    IEventSerializer serializer,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    ILogger<ServiceBusEventBus> logger) : IEventBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client = client;
    private readonly IEventSerializer _serializer = serializer;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly EventBusOptions _options = options.Value;
    private readonly ILogger<ServiceBusEventBus> _logger = logger;
    private readonly ConcurrentBag<ServiceBusProcessor> _processors = new();

    public async Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        string topicName = BuildTopicName(topic);

        ServiceBusSender sender = _client.CreateSender(topicName);
        ServiceBusMessage message = new(_serializer.Serialize(@event))
        {
            MessageId = @event.Id.ToString(),
            ContentType = "application/json",
            Subject = typeof(TEvent).Name
        };

        message.ApplicationProperties["type"] = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        message.ApplicationProperties["occurredOn"] = @event.OccurredOn;

        _logger.LogInformation("Publishing event {EventType} to topic {Topic}", typeof(TEvent).Name, topicName);

        await sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task SubscribeAsync<TEvent, THandler>(string topic, string subscription, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        string topicName = BuildTopicName(topic);
        string subscriptionName = BuildSubscriptionName(subscription);

        ServiceBusProcessorOptions processorOptions = new()
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        };

        ServiceBusProcessor processor = _client.CreateProcessor(topicName, subscriptionName, processorOptions);

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                TEvent? deserialized = _serializer.Deserialize<TEvent>(args.Message.Body.ToString());
                if (deserialized is null)
                {
                    _logger.LogWarning("Received null event while processing topic {Topic} subscription {Subscription}", topicName, subscriptionName);
                    await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
                    return;
                }

                using IServiceScope scope = _serviceProvider.CreateScope();
                THandler handler = scope.ServiceProvider.GetRequiredService<THandler>();

                _logger.LogInformation("Handling event {EventType} from topic {Topic} subscription {Subscription}", typeof(TEvent).Name, topicName, subscriptionName);
                await handler.HandleAsync(deserialized, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventType} from topic {Topic} subscription {Subscription}", typeof(TEvent).Name, topicName, subscriptionName);
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Service Bus processor error. EntityPath={EntityPath}, ErrorSource={ErrorSource}", args.EntityPath, args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(cancellationToken);
        _processors.Add(processor);

        _logger.LogInformation("Subscribed to topic {Topic} subscription {Subscription}", topicName, subscriptionName);
    }

    public async ValueTask DisposeAsync()
    {
        while (_processors.TryTake(out ServiceBusProcessor? processor))
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }
    }

    private string BuildTopicName(string topic) => string.IsNullOrWhiteSpace(_options.ServiceBus.TopicPrefix)
        ? topic
        : string.Concat(_options.ServiceBus.TopicPrefix, topic);

    private string BuildSubscriptionName(string subscription) => string.IsNullOrWhiteSpace(_options.ServiceBus.SubscriptionPrefix)
        ? subscription
        : string.Concat(_options.ServiceBus.SubscriptionPrefix, subscription);
}
