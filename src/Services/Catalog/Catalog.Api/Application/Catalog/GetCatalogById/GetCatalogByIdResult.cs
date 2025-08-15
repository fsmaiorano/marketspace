using System.Collections.ObjectModel;

namespace Catalog.Api.Application.Catalog.GetCatalogById;

public class GetCatalogByIdResult(
    Guid id,
    string name,
    string description,
    string imageUrl,
    decimal price,
    ReadOnlyCollection<string> categories)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Description { get; init; } = description;
    public string ImageUrl { get; init; } = imageUrl;
    public decimal Price { get; init; } = price;
    public ReadOnlyCollection<string> Categories { get; init; } = categories;
}