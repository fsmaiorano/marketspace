using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Integration event published when a question's best answer is chosen
/// </summary>
public record QuestionBestAnswerChosenIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public required string QuestionId { get; init; }
    public required string BestAnswerId { get; init; }
    public required string QuestionAuthorId { get; init; }
    public required string AnswerAuthorId { get; init; }
    public required string QuestionTitle { get; init; }
}

