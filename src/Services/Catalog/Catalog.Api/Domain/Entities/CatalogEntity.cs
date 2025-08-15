using BuildingBlocks.Abstractions;
using Catalog.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Catalog.Api.Domain.Entities;

public class CatalogEntity : Aggregate<CatalogId>
{
    public string Name { get; private set; } = string.Empty;
    public ReadOnlyCollection<string> Categories => _categories.AsReadOnly();
    private readonly List<string> _categories = [];
    public string Description { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public Price Price { get; private set; } = null!;
    public new DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static CatalogEntity Create(
        string name,
        IEnumerable<string> categories,
        string description,
        string imageUrl,
        Price price)
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

        var entity = new CatalogEntity
        {
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            Price = price,
            CreatedAt = DateTimeOffset.UtcNow
        };
        entity._categories.AddRange(categories.Distinct());
        return entity;
    }

    public void AddCategory(string category)
    {
        if (!string.IsNullOrWhiteSpace(category) && !_categories.Contains(category))
            _categories.Add(category);
    }

    public void RemoveCategory(string category)
    {
        _categories.Remove(category);
    }

    public bool HasCategory(string category)
    {
        return _categories.Contains(category);
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
        _categories.Clear();
        _categories.AddRange(categories.Distinct());
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}