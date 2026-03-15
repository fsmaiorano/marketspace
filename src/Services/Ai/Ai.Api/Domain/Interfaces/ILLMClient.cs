namespace Ai.Api.Domain.Interfaces;

public interface ILLMClient
{
    Task<string> Generate(string prompt);
}   