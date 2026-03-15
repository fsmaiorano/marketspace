using Ai.Api.Domain.Vectors;

namespace Ai.Test.Mocks;

public class MockVectorStore : IVectorStore
{
    private readonly List<(string Content, float[] Embedding, string? ContextId)> _documents = [];

    public Task<IEnumerable<string>> Search(float[] vector, string? contextId = null, int limit = 5)
    {
        IEnumerable<string> results = _documents
            .Where(d => contextId is null || d.ContextId == contextId)
            .Take(limit)
            .Select(d => d.Content);

        return Task.FromResult(results);
    }

    public Task Upsert(string content, float[] embedding, string? contextId = null, string? metadata = null)
    {
        _documents.Add((content, embedding, contextId));
        return Task.CompletedTask;
    }
}
