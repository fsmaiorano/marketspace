using Ai.Api.Application.Tools;
using Ai.Api.Domain;

namespace Ai.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ChatUseCase>();
        services.AddScoped<AskWithRagUseCase>();
        services.AddScoped<AgentUseCase>();
        services.AddScoped<IngestDocumentsUseCase>();
        services.AddHttpClient<GetOrderStatusTool>();
        services.AddHttpClient<GetOrdersByCustomerTool>();
        services.AddHttpClient<SearchProductsTool>();
        services.AddSingleton<ConversationStore>();
        return services;
    }
}