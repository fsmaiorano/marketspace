using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Services.CurrentUser;

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
