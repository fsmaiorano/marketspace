using System.Text.Json.Serialization;

namespace Catalog.Api.Domain.ValueObjects;

public record Stock
{
    public int Value { get; init; }

    [JsonConstructor]
    private Stock(int value) => Value = value;

    public static Stock Of(int value)
    {
        return value < 0 ? throw new ArgumentException("Stock cannot be negative.", nameof(value)) : new Stock(value);
    }
}