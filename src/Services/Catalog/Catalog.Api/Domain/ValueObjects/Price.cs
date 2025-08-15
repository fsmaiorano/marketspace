namespace Catalog.Api.Domain.ValueObjects;

public record Price
{
    public decimal Value { get; }

    private Price(decimal value) => Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    public static Price Of(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(value));
        if (decimal.Round(value, 2) != value)
            throw new ArgumentException("Price must have at most 2 decimal places.", nameof(value));
        return new Price(value);
    }
}