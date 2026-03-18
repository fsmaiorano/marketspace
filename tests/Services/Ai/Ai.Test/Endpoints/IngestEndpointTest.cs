using Ai.Api.Domain.Vectors;
using Ai.Test.Fixtures;

namespace Ai.Test.Endpoints;

public class IngestEndpointTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Documents_Are_Valid()
    {
        IngestRequest request = AiBuilder.CreateIngestRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/rag/ingest", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_Ingest_Response_With_Correct_Count()
    {
        IngestRequest request = new()
        {
            Documents = ["First document.", "Second document.", "Third document."],
            ContextId = "test-context"
        };

        HttpResponseMessage response = await DoPost("/rag/ingest", request);
        IngestResponse? result = await response.Content.ReadFromJsonAsync<IngestResponse>();

        result.Should().NotBeNull();
        result!.Ingested.Should().Be(3);
    }

    [Fact]
    public async Task Returns_Zero_When_Documents_List_Is_Empty()
    {
        IngestRequest request = new() { Documents = [], ContextId = "test-context" };

        HttpResponseMessage response = await DoPost("/rag/ingest", request);
        IngestResponse? result = await response.Content.ReadFromJsonAsync<IngestResponse>();

        result.Should().NotBeNull();
        result!.Ingested.Should().Be(0);
    }

    [Fact]
    public async Task Skips_Blank_Documents_And_Reports_Real_Count()
    {
        IngestRequest request = new()
        {
            Documents = ["Valid document.", "   ", "", "Another valid document."],
            ContextId = "test-context"
        };

        HttpResponseMessage response = await DoPost("/rag/ingest", request);
        IngestResponse? result = await response.Content.ReadFromJsonAsync<IngestResponse>();

        result.Should().NotBeNull();
        result!.Ingested.Should().Be(2, "blank and whitespace-only entries should be skipped");
    }

    [Fact]
    public async Task Ingested_Documents_Are_Searchable_Via_Rag()
    {
        string contextId = $"ctx-{Guid.NewGuid():N}";
        const string documentContent = "MarketSpace refund policy allows returns within 30 days.";

        IngestRequest ingestRequest = new()
        {
            Documents = [documentContent],
            ContextId = contextId
        };
        await DoPost("/rag/ingest", ingestRequest);

        RagRequest ragRequest = new()
        {
            Question = "What is the return policy?",
            ContextId = contextId
        };

        HttpResponseMessage ragResponse = await DoPost("/rag", ragRequest);
        RagResponse? result = await ragResponse.Content.ReadFromJsonAsync<RagResponse>();

        result.Should().NotBeNull();
        result!.Sources.Should().ContainSingle(s => s == documentContent,
            "the ingested document should appear as a source in the RAG response");
    }

    [Fact]
    public async Task Ingested_Documents_Are_Stored_In_Vector_Store()
    {
        IVectorStore vectorStore = fixture.Services.GetRequiredService<IVectorStore>();
        string contextId = $"ctx-{Guid.NewGuid():N}";

        IngestRequest request = new()
        {
            Documents = ["Policy A.", "Policy B."],
            ContextId = contextId
        };

        await DoPost("/rag/ingest", request);

        IEnumerable<string> results = await vectorStore.Search(new float[768], contextId, limit: 10);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task Returns_CorrelationId_Header_In_Response()
    {
        IngestRequest request = AiBuilder.CreateIngestRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/rag/ingest", request);

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue(
            "every response must include the X-Correlation-ID tracing header");
    }
}
