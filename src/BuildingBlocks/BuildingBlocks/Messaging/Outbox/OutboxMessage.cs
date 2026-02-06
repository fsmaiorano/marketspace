namespace BuildingBlocks.Messaging.Outbox;

public class OutboxMessage(
    Guid id,
    DateTime occurredOn,
    string type,
    string content)
{
    public Guid Id { get; init; } = id;
    public DateTime OccurredOn { get; init; } = occurredOn;
    public string Type { get; init; } = type;
    public string Content { get; init; } = content;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}

