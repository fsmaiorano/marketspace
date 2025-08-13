namespace BuildingBlocks.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }

    public List<string> Errors { get; } = [];
}