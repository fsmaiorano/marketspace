namespace Ai.Api.Domain.Embeddings;

public interface IEmbeddingGenerator
{
    Task<float[]> Generate(string text);
}