using BackendForFrontend.Api.Ai.Services;
using BackendForFrontend.Api.Ai.UseCases;

namespace BackendForFrontend.Api.Ai;

public static class DependencyInjection
{
    public static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        services.AddScoped<AiUseCase>();
        services.AddHttpClient<AiService>();
        return services;
    }
}