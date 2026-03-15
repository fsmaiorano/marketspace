using Ai.Api.Domain.Interfaces;

namespace Ai.Test.Mocks;

public class MockLLMClient : ILLMClient
{
    public const string FixedResponse = "This is a mock AI response for testing purposes.";

    public Task<string> Generate(string prompt) =>
        Task.FromResult(FixedResponse);
}
