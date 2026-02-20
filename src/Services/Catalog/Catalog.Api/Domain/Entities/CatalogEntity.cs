using BuildingBlocks.Abstractions;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Domain.Entities;

public class CatalogEntity : Aggregate<CatalogId>
{
    public string Name { get; private set; } = string.Empty;
    public List<string> Categories { get; private set; } = [];
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

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required.", nameof(imageUrl));

        if (merchantId == Guid.Empty)
            throw new ArgumentException("MerchantId cannot be empty.", nameof(merchantId));
        
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(price);

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

    private void AddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        if (!Categories.Contains(category))
        {
            Categories.Add(category);
        }
    }

    private void RemoveCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        Categories.Remove(category);
    }

    private void ClearCategories() => Categories.Clear();

    private void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (Name == name)
            return;

        Name = name;
    }

    private void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (Description == description)
            return;

        Description = description;
    }

    private void ChangeImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required.", nameof(imageUrl));

        if (ImageUrl == imageUrl)
            return;

        ImageUrl = imageUrl;
    }

    private void ChangePrice(Price price)
    {
        if (Price == price)
            return;

        Price = price;
    }

    private void ChangeMerchantId(Guid merchantId)
    {
        if (MerchantId == merchantId)
            return;

        MerchantId = merchantId;
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    public void Update(
        string? name,
        IEnumerable<string>? categories,
        string? description,
        string? imageUrl,
        Price? price,
        Guid? merchantId = null)
    {
        if (name is not null)
            ChangeName(name);

        if (categories is not null)
        {
            ClearCategories();
            foreach (string category in categories)
                AddCategory(category);
        }

        if (description is not null)
            ChangeDescription(description);

        if (imageUrl is not null)
            ChangeImageUrl(imageUrl);

        if (price is not null)
            ChangePrice(price);

        if (merchantId is not null)
            ChangeMerchantId(merchantId.Value);

        Touch();
    }
}