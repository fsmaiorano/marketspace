using System.Text.Json.Serialization;

namespace Order.Api.Domain.ValueObjects;

public record Price
{
    public decimal Value { get; set; }

    private Price(decimal value) => Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    [JsonConstructor]
    public Price() { }

    public static Price Of(decimal value)
    {
        return value < 0 ? throw new ArgumentException("Price cannot be negative.", nameof(value)) : new Price(value);
    }
}