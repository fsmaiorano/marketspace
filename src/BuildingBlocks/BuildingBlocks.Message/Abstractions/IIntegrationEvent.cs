namespace BuildingBlocks.Message.Abstractions;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOn { get; }
}

public abstract record IntegrationEvent(Guid Id, DateTimeOffset OccurredOn) : IIntegrationEvent
{
    protected IntegrationEvent() : this(Guid.CreateVersion7(), DateTimeOffset.UtcNow)
    {
    }
}
