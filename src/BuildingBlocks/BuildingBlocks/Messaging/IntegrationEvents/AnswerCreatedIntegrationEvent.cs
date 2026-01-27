using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Integration event published when a new answer is created
/// </summary>
public record AnswerCreatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public required string AnswerId { get; init; }
    public required string QuestionId { get; init; }
    public required string AuthorId { get; init; }
    public required string Content { get; init; }
}

