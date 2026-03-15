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
}
