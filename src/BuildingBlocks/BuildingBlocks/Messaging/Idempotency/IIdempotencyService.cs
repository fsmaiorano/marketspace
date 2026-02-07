namespace BuildingBlocks.Messaging.Idempotency;

public interface IIdempotencyService
{
    /// <summary>
    /// Executes the handler if the event has not been processed yet.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="eventName">The name of the event type.</param>
    /// <param name="handler">The handler to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(Guid eventId, string eventName, Func<CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}



