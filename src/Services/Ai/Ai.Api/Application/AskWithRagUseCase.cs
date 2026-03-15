using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Interfaces;
using Ai.Api.Domain.Vectors;
using Ai.Api.Endpoints.Dtos;

namespace Ai.Api.Application;

public class AskWithRagUseCase(
    ILLMClient llmClient,
    IEmbeddingGenerator embeddingGenerator,
    IVectorStore vectorStore)
{
    public async Task<RagResponse> AskAsync(RagRequest request)
    {
        float[] queryEmbedding = await embeddingGenerator.Generate(request.Question ?? string.Empty);

        IEnumerable<string> contextDocs = await vectorStore.Search(queryEmbedding, request.ContextId, limit: 4);

        List<string> sources = [.. contextDocs];

        string context = sources.Count > 0
            ? string.Join("\n\n", sources)
            : "No relevant documents found.";

        string prompt = $"""
            You are a helpful assistant for MarketSpace. Use the following context to answer the question.
            If the context does not contain the answer, say you don't know.

            Context:
            {context}

            Question: {request.Question}
            Answer:
            """;

        string answer = await llmClient.Generate(prompt);

        return new RagResponse
        {
            Answer = answer.Trim(),
            Sources = sources
        };
    }
}