using Ai.Test.Fixtures;

namespace Ai.Test.Endpoints;

public class AgentEndpointTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_For_Generic_Message()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/agent", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_AgentResponse_With_Answer()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.Answer.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_Message_Has_No_Order_Id()
    {
        AgentRequest request = new()
        {
            Message = "What products are available in MarketSpace?",
            UserId = Guid.NewGuid().ToString()
        };

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.UsedTools.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_Order_Is_Not_Found()
    {
        // Order API is not available in tests, so the tool returns null → UsedTools stays false
        Guid nonExistentOrderId = Guid.NewGuid();
        AgentRequest request = AiBuilder.CreateAgentRequestWithOrderId(nonExistentOrderId);

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.UsedTools.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_Message_Contains_Word_Order_But_No_Guid()
    {
        AgentRequest request = new()
        {
            Message = "I want to place an order for some electronics.",
            UserId = Guid.NewGuid().ToString()
        };

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.UsedTools.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_CorrelationId_Header_In_Response()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestFaker().Generate();

        HttpResponseMessage response = await DoPost("/agent", request);

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue(
            "every response must include the X-Correlation-ID tracing header");
    }
}
