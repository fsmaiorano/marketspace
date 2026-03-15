namespace Ai.Api.Domain.Interfaces;

public interface ILlmClient
{
    Task<string> Generate(string prompt);
}   