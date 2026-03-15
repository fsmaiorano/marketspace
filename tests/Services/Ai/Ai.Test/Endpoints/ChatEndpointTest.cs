using Ai.Test.Fixtures;
using Ai.Test.Mocks;

namespace Ai.Test.Endpoints;

public class ChatEndpointTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Message_Is_Valid()
    {
        ChatRequest request = AiBuilder.CreateChatRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/chat", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_ChatResponse_With_Non_Empty_Answer()
    {
        ChatRequest request = AiBuilder.CreateChatRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/chat", request);
        ChatResponse? result = await response.Content.ReadFromJsonAsync<ChatResponse>();

        result.Should().NotBeNull();
        result!.Answer.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Returns_Answer_From_LLM()
    {
        ChatRequest request = AiBuilder.CreateChatRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/chat", request);
        ChatResponse? result = await response.Content.ReadFromJsonAsync<ChatResponse>();

        result!.Answer.Should().Be(MockLLMClient.FixedResponse);
    }

    [Fact]
    public async Task Returns_CorrelationId_Header_In_Response()
    {
        ChatRequest request = AiBuilder.CreateChatRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/chat", request);

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue(
            "every response must include the X-Correlation-ID tracing header");
    }

    [Fact]
    public async Task Returns_Ok_When_UserId_Is_Null()
    {
        ChatRequest request = new() { Message = "Hello, what is MarketSpace?", UserId = null };

        HttpResponseMessage response = await DoPost("/chat", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
