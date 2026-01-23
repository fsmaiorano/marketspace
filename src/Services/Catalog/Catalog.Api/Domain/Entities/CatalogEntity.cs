using BuildingBlocks.Abstractions;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Domain.Entities;

public class CatalogEntity : Aggregate<CatalogId>
{
    public string Name { get; private set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
    public string Description { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public Price Price { get; private set; } = null!;
    public Guid MerchantId { get; private set; } = Guid.Empty;
    public new DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static CatalogEntity Create(
        string name,
        IEnumerable<string> categories,
        string description,
        string imageUrl,
        Price price,
        Guid merchantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required.", nameof(imageUrl));

        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (merchantId == Guid.Empty)
            throw new ArgumentException("MerchantId cannot be empty.", nameof(merchantId));

        CatalogEntity entity = new()

        {
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            Price = price,
            MerchantId = merchantId,
            Categories = [.. categories.Distinct()],
            CreatedAt = DateTimeOffset.UtcNow
        };

        return entity;
    }

    public void AddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        if (!Categories.Contains(category))
        {
            Categories.Add(category);
        }
    }

    public void RemoveCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        Categories.Remove(category);
    }

    public void Update(
        string name,
        IEnumerable<string> categories,
        string description,
        string imageUrl,
        Price price)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Price = price;
        Categories = new List<string>(categories.Distinct());
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}