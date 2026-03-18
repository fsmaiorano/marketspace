using Ai.Api.Domain;
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

    // --- SearchProductsTool tests ---

    [Fact]
    public async Task Returns_Ok_When_Message_Asks_About_Products()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestForProducts();

        HttpResponseMessage response = await DoPost("/agent", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_Answer_When_Product_Search_Fails_Gracefully()
    {
        // Catalog API is not available in tests → SearchProductsTool returns null → agent answers without tool
        AgentRequest request = AiBuilder.CreateAgentRequestForProducts();

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.Answer.Should().NotBeNullOrWhiteSpace();
        result.UsedTools.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_Catalog_Api_Is_Unavailable()
    {
        AgentRequest request = new()
        {
            Message = "Show me all items in the catalog",
            UserId = Guid.NewGuid().ToString()
        };

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result!.UsedTools.Should().BeFalse();
    }

    // --- GetOrdersByCustomerTool tests ---

    [Fact]
    public async Task Returns_Ok_When_Message_Asks_For_My_Orders()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestForMyOrders(Guid.NewGuid().ToString());

        HttpResponseMessage response = await DoPost("/agent", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_My_Orders_Api_Is_Unavailable()
    {
        AgentRequest request = AiBuilder.CreateAgentRequestForMyOrders(Guid.NewGuid().ToString());

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.UsedTools.Should().BeFalse();
    }

    [Fact]
    public async Task Returns_UsedTools_False_When_My_Orders_Requested_Without_UserId()
    {
        AgentRequest request = new() { Message = "Show me my recent orders", UserId = null };

        HttpResponseMessage response = await DoPost("/agent", request);
        AgentResponse? result = await response.Content.ReadFromJsonAsync<AgentResponse>();

        result.Should().NotBeNull();
        result!.UsedTools.Should().BeFalse();
    }

    // --- Conversation memory tests ---

    [Fact]
    public async Task Conversation_Memory_Accumulates_History_For_Same_User()
    {
        string userId = Guid.NewGuid().ToString();
        ConversationStore store = fixture.Services.GetRequiredService<ConversationStore>();

        AgentRequest first = new() { Message = "Hello, what is MarketSpace?", UserId = userId };
        await DoPost("/agent", first);

        IReadOnlyList<ConversationMessage> history = store.GetHistory(userId);

        history.Should().HaveCount(2, "one user turn and one assistant turn should be stored");
        history[0].Role.Should().Be("user");
        history[1].Role.Should().Be("assistant");
    }

    [Fact]
    public async Task Conversation_Memory_Is_Isolated_Per_User()
    {
        string userA = Guid.NewGuid().ToString();
        string userB = Guid.NewGuid().ToString();
        ConversationStore store = fixture.Services.GetRequiredService<ConversationStore>();

        await DoPost("/agent", new AgentRequest { Message = "Hello from user A", UserId = userA });

        IReadOnlyList<ConversationMessage> historyB = store.GetHistory(userB);

        historyB.Should().BeEmpty("user B should have no history from user A's messages");
    }

    [Fact]
    public async Task Conversation_Memory_Grows_Across_Multiple_Turns()
    {
        string userId = Guid.NewGuid().ToString();
        ConversationStore store = fixture.Services.GetRequiredService<ConversationStore>();

        await DoPost("/agent", new AgentRequest { Message = "First message", UserId = userId });
        await DoPost("/agent", new AgentRequest { Message = "Second message", UserId = userId });

        IReadOnlyList<ConversationMessage> history = store.GetHistory(userId);

        history.Should().HaveCount(4, "two turns × (user + assistant) = 4 messages");
    }

    [Fact]
    public async Task Returns_Ok_For_Anonymous_User_Without_UserId()
    {
        AgentRequest request = new() { Message = "What is MarketSpace?", UserId = null };

        HttpResponseMessage response = await DoPost("/agent", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

