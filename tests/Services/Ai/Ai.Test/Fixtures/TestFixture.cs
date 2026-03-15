using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Interfaces;
using Ai.Api.Domain.Vectors;
using Ai.Api.Infrastructure.VectorStore;
using Ai.Test.Mocks;
using BaseTest.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ai.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<AiProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // Remove all hosted services (VectorSeeder calls Ollama on startup)
            services.RemoveAll<IHostedService>();

            // Replace Ollama LLM client with a fast in-process mock
            services.RemoveAll<ILLMClient>();
            services.AddSingleton<ILLMClient, MockLLMClient>();

            // Replace Ollama embedding generator with a mock
            services.RemoveAll<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>();

            // Replace pgvector store with a fast in-memory mock
            services.RemoveAll<IVectorStore>();
            services.RemoveAll<PgVectorStore>();
            services.AddSingleton<IVectorStore, MockVectorStore>();
        });
    }
}
