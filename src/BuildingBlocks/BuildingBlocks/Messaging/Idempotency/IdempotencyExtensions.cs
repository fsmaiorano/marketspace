using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Idempotency;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddIdempotency<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        services.AddScoped<IIdempotencyService, IdempotencyService<TContext>>();
        return services;
    }
}

