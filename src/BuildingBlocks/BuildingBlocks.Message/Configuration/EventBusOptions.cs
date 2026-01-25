namespace BuildingBlocks.Message.Configuration;

public sealed class EventBusOptions
{
    public const string SectionName = "MessageBroker";

    public EventBusProvider Provider { get; set; } = EventBusProvider.AzureServiceBus;

    public ServiceBusOptions ServiceBus { get; set; } = new();
}

public sealed class ServiceBusOptions
{
    public string? ConnectionString { get; set; }
    public string TopicPrefix { get; set; } = string.Empty;
    public string SubscriptionPrefix { get; set; } = string.Empty;
}

public enum EventBusProvider
{
    AzureServiceBus = 0
}
