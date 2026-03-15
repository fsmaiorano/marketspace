using Ai.Api.Application.Tools;

namespace Ai.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ChatUseCase>();
        services.AddScoped<AskWithRagUseCase>();
        services.AddScoped<AgentUseCase>();
        services.AddHttpClient<GetOrderStatusTool>();
        return services;
    }
}