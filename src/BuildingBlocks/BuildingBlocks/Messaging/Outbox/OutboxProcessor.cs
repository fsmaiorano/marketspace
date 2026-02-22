using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Messaging.Outbox;

public class OutboxProcessor<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxProcessor<TDbContext>> logger)
    : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        IDomainEventDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            List<OutboxMessage> messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOn == null)
                .Take(20)
                .ToListAsync(stoppingToken) ?? [];

            foreach (OutboxMessage message in messages)
            {
                try
                {
                    logger.LogInformation("Processing outbox message {Id} of type {Type}", message.Id, message.Type);

                    Type? type = Type.GetType(message.Type);
                    if (type == null)
                    {
                        logger.LogError("Could not resolve type {Type}", message.Type);
                        message.Error = $"Could not resolve type {message.Type}";
                        message.ProcessedOn = DateTime.UtcNow;
                        continue;
                    }

                    logger.LogDebug("Message content: {Content}", message.Content);

                    JsonSerializerOptions jsonOptions = new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true,
                        IncludeFields = true
                    };

                    object? domainEventObj = JsonSerializer.Deserialize(message.Content, type, jsonOptions);
                    if (domainEventObj is not IDomainEvent domainEvent)
                    {
                        logger.LogError(
                            "Message {Id} is not a valid domain event. Deserialized object type: {ObjectType}",
                            message.Id, domainEventObj?.GetType().Name ?? "null");
                        message.Error = "Invalid domain event";
                        message.ProcessedOn = DateTime.UtcNow;
                        continue;
                    }

                    logger.LogInformation("Dispatching domain event {EventType} from message {MessageId}",
                        domainEvent.GetType().Name, message.Id);

                    await dispatcher.DispatchAsync(domainEvent, stoppingToken);

                    message.ProcessedOn = DateTime.UtcNow;
                    logger.LogInformation("Successfully processed message {Id}", message.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process message {Id}. Error: {Error}, StackTrace: {StackTrace}",
                        message.Id, ex.Message, ex.StackTrace);
                    message.Error = $"{ex.Message} | {ex.StackTrace}";
                    message.ProcessedOn = DateTime.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        });
    }
}