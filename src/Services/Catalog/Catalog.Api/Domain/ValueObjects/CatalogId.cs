namespace Catalog.Api.Domain.ValueObjects;

public record CatalogId
{
    public Guid Value { get; init; }

    private CatalogId(Guid value) => Value = value;

    public static CatalogId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CatalogId cannot be empty.", nameof(value));

        return new CatalogId(value);
    }
}