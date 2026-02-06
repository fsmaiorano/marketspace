using System.Text.Json.Serialization;

namespace Order.Api.Domain.ValueObjects;

public record CustomerId
{
    public Guid Value { get; set; }

    private CustomerId(Guid value) => Value = value;

    [JsonConstructor]
    public CustomerId() { }

    public static CustomerId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(value));

        return new CustomerId(value);
    }
}