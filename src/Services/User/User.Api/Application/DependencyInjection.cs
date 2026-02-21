using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Api.Services;

namespace User.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Application-level registrations
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}

