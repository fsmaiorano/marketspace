using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Vectors;
using Ai.Test.Fixtures;
using Ai.Test.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Ai.Test.Endpoints;

public class RagEndpointTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Question_Is_Valid()
    {
        RagRequest request = AiBuilder.CreateRagRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/rag", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_RagResponse_With_Answer()
    {
        RagRequest request = AiBuilder.CreateRagRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/rag", request);
        RagResponse? result = await response.Content.ReadFromJsonAsync<RagResponse>();

        result.Should().NotBeNull();
        result!.Answer.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Returns_RagResponse_With_Sources_When_Documents_Exist()
    {
        // Seed a document into the in-memory vector store
        IVectorStore vectorStore = fixture.Services.GetRequiredService<IVectorStore>();
        IEmbeddingGenerator embedder = fixture.Services.GetRequiredService<IEmbeddingGenerator>();

        float[] embedding = await embedder.Generate("Order lifecycle in MarketSpace");
        await vectorStore.Upsert(
            "Orders in MarketSpace go from Created to Processing to Delivered.",
            embedding,
            contextId: "marketspace-docs");

        RagRequest request = new()
        {
            Question = "What are the order statuses?",
            ContextId = "marketspace-docs"
        };

        HttpResponseMessage response = await DoPost("/rag", request);
        RagResponse? result = await response.Content.ReadFromJsonAsync<RagResponse>();

        result.Should().NotBeNull();
        result!.Sources.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Returns_Empty_Sources_When_No_Documents_In_Context()
    {
        RagRequest request = new()
        {
            Question = "What is shipping policy?",
            ContextId = "nonexistent-context"
        };

        HttpResponseMessage response = await DoPost("/rag", request);
        RagResponse? result = await response.Content.ReadFromJsonAsync<RagResponse>();

        result.Should().NotBeNull();
        result!.Sources.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_CorrelationId_Header_In_Response()
    {
        RagRequest request = AiBuilder.CreateRagRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/rag", request);

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue(
            "every response must include the X-Correlation-ID tracing header");
    }
}
