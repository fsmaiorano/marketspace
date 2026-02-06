using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Outbox;

public static class OutboxExtensions
{
    public static IServiceCollection AddOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        return services;
    }
}

