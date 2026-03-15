using Ai.Api.Domain.Embeddings;

namespace Ai.Test.Mocks;

public class MockEmbeddingGenerator : IEmbeddingGenerator
{
    public Task<float[]> Generate(string text) =>
        Task.FromResult(new float[768]);
}
