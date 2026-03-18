using Ai.Api.Endpoints.Dtos;
using Bogus;

namespace Builder;

public static class AiBuilder
{
    public static Faker<ChatRequest> CreateChatRequestFaker() =>
        new Faker<ChatRequest>()
            .RuleFor(r => r.Message, f => f.Lorem.Sentence())
            .RuleFor(r => r.UserId, f => f.Random.Guid().ToString());

    public static Faker<RagRequest> CreateRagRequestFaker() =>
        new Faker<RagRequest>()
            .RuleFor(r => r.Question, f => f.Lorem.Sentence() + "?")
            .RuleFor(r => r.ContextId, _ => "marketspace-docs");

    public static Faker<AgentRequest> CreateAgentRequestFaker() =>
        new Faker<AgentRequest>()
            .RuleFor(r => r.Message, f => f.Lorem.Sentence())
            .RuleFor(r => r.UserId, f => f.Random.Guid().ToString());

    public static AgentRequest CreateAgentRequestWithOrderId(Guid orderId) =>
        new() { Message = $"What is the status of order {orderId}?", UserId = Guid.NewGuid().ToString() };

    public static AgentRequest CreateAgentRequestForMyOrders(string userId) =>
        new() { Message = "Show me my recent orders", UserId = userId };

    public static AgentRequest CreateAgentRequestForProducts() =>
        new() { Message = "What products do you have available?", UserId = Guid.NewGuid().ToString() };

    public static Faker<IngestRequest> CreateIngestRequestFaker() =>
        new Faker<IngestRequest>()
            .RuleFor(r => r.Documents, f => [f.Lorem.Paragraph(), f.Lorem.Paragraph()])
            .RuleFor(r => r.ContextId, f => f.Random.Word().ToLower())
            .RuleFor(r => r.Metadata, f => f.Random.Word());
}
