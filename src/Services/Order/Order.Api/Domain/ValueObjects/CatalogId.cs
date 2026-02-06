using System.Text.Json.Serialization;

namespace Order.Api.Domain.ValueObjects;

public record CatalogId
{
    public Guid Value { get; set; }

    private CatalogId(Guid value) => Value = value;

    [JsonConstructor]
    public CatalogId() { }

    public static CatalogId Of(Guid value)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("CatalogId cannot be empty.", nameof(value))
            : new CatalogId(value);
    }
}