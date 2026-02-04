using System.Text.Json.Serialization;

namespace BuildingBlocks;

[method: JsonConstructor]
public class Result<T>(bool isSuccess, string? error, T? data)
{
    public bool IsSuccess { get; } = isSuccess;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; } = error;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; } = data;

    private Result(T data) : this(true, null, data)
    {
    }

    private Result(string error) : this(false, error, default(T?))
    {
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}