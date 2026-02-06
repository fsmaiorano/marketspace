using BuildingBlocks.Abstractions;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Messaging.Outbox;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        List<OutboxMessage> outboxMessages = [.. dbContext.ChangeTracker
            .Entries<IAggregate>()
            .Select(x => x.Entity)
            .SelectMany(aggregate =>
            {
                IDomainEvent[] domainEvents = aggregate.ClearDomainEvents();

                return domainEvents.Select(domainEvent => new OutboxMessage(
                    id: Guid.NewGuid(),
                    occurredOn: domainEvent.OccurredAt,
                    type: domainEvent.GetType().AssemblyQualifiedName!,
                    content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), options: new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                ));
            })];

        if (outboxMessages.Count != 0)
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}


