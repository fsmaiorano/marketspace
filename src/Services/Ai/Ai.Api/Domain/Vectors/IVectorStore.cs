namespace Ai.Api.Domain.Vectors;

public interface IVectorStore
{
    Task<IEnumerable<string>> Search(float[] vector, string? contextId = null, int limit = 5);
    Task Upsert(string content, float[] embedding, string? contextId = null, string? metadata = null);
}