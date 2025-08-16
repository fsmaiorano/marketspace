namespace Catalog.Api.Application.Catalog.CreateCatalog;

public class CreateCatalogCommand
{
    public string Name { get; set; }

    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; } = 0.0m;
    public List<string> Categories { get; set; }

    public CreateCatalogCommand(string name, string description, string imageUrl, decimal price,
        List<string> categories)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Price = price;
        Categories = categories;
    }
}