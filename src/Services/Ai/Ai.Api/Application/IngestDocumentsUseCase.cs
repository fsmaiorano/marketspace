using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Vectors;
using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Application;

public class IngestDocumentsUseCase(
    IEmbeddingGenerator embeddingGenerator,
    IVectorStore vectorStore)
{
    public async Task<IngestResponse> IngestAsync(IngestRequest request)
    {
        if (request.Documents is null || request.Documents.Count == 0)
            return new IngestResponse { Ingested = 0 };

        int count = 0;
        foreach (string document in request.Documents.Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            float[] embedding = await embeddingGenerator.Generate(document);
            await vectorStore.Upsert(document, embedding, contextId: request.ContextId,
                metadata: request.Metadata);
            count++;
        }

        return new IngestResponse { Ingested = count };
    }
}
