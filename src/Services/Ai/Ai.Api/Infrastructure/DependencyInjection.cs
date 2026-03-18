using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Interfaces;
using Ai.Api.Domain.Vectors;
using Ai.Api.Infrastructure.Embeddings;
using Ai.Api.Infrastructure.LLM;
using Ai.Api.Infrastructure.VectorStore;

namespace Ai.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpClient<ILLMClient, OllamaClient>();
        services.AddHttpClient<IEmbeddingGenerator, OllamaEmbedding>();
        services.AddSingleton<IVectorStore, PgVectorStore>();
        services.AddHostedService<VectorSeeder>();
        services.AddHostedService<CatalogSeeder>();
        return services;
    }
}