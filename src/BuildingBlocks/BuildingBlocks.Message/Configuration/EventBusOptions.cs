namespace BuildingBlocks.Message.Configuration;

public sealed class EventBusOptions
{
    public const string SectionName = "MessageBroker";

    public EventBusProvider Provider { get; set; } = EventBusProvider.AzureServiceBus;

    public ServiceBusOptions ServiceBus { get; set; } = new();
    public RabbitMqOptions RabbitMq { get; set; } = new();
}

public sealed class ServiceBusOptions
{
    public string? ConnectionString { get; set; }
    public string TopicPrefix { get; set; } = string.Empty;
    public string SubscriptionPrefix { get; set; } = string.Empty;
}

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "marketspace.exchange";
    public string ExchangeType { get; set; } = "topic";
    public string QueuePrefix { get; set; } = string.Empty;
}

public enum EventBusProvider
{
    AzureServiceBus = 0,
    RabbitMq = 1
}
