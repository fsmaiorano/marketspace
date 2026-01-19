using Bogus;
using Catalog.Api.Application.Catalog.CreateCatalog;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;

namespace Builder;

public static class CatalogBuilder
{
    private static readonly string[] ProductCategories =
    {
        "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Toys", "Beauty", "Automotive",
        "Food & Beverages", "Health", "Music", "Movies", "Gaming", "Office Supplies", "Pet Supplies", "Jewelry",
        "Tools", "Software"
    };

    public static Faker<CatalogEntity> CreateCatalogFaker(Guid merchantId = default)
    {
        return new Faker<CatalogEntity>()
            .RuleFor(m => m.Id, f => CatalogId.Of(f.Random.Guid()))
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.ImageUrl, f => f.Image.PicsumUrl(800, 600))
            .RuleFor(m => m.Price, f => Price.Of(f.Finance.Amount(1, 1000, 2)))
            .RuleFor(m => m.Categories,
                f => f.PickRandom(ProductCategories, f.Random.Int(1, 10)).ToList())
            .RuleFor(m => m.MerchantId, f => merchantId == default ? f.Random.Guid() : merchantId);
    }


    public static Faker<CreateCatalogCommand> CreateCreateCatalogCommandFaker(string email = "")
    {
        return new Faker<CreateCatalogCommand>()
            .CustomInstantiator(f => new CreateCatalogCommand(
                name: f.Company.CompanyName(),
                description: f.Lorem.Sentence(),
                imageUrl: f.Image.PicsumUrl(800, 600),
                price: f.Finance.Amount(1, 1000, 2),
                categories: f.PickRandom(ProductCategories, f.Random.Int(1, 10)).ToList(),
                merchantId: f.Random.Guid()
            ));
    }

    public static Faker<UpdateCatalogCommand> CreateUpdateCatalogCommandFaker(Guid? id = null, string email = "")
    {
        return new Faker<UpdateCatalogCommand>()
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.ImageUrl, f => f.Image.PicsumUrl(800, 600))
            .RuleFor(m => m.Price, f => f.Finance.Amount(1, 1000, 2))
            .RuleFor(m => m.Categories, f => f.PickRandom(ProductCategories, f.Random.Int(1, 10)));
    }

    public static Faker<DeleteCatalogCommand> CreateDeleteCatalogCommandFaker(Guid? id = null)
    {
        return new Faker<DeleteCatalogCommand>()
            .RuleFor(m => m.Id, f => id ?? f.Random.Guid());
    }

    public static CatalogEntity GenerateCatalog(string email = "") =>
        CreateCatalogFaker().Generate();

    public static CreateCatalogCommand GenerateCreateCommand(string email = "") =>
        CreateCreateCatalogCommandFaker(email).Generate();

    public static UpdateCatalogCommand GenerateUpdateCommand(Guid? id = null, string email = "") =>
        CreateUpdateCatalogCommandFaker(id, email).Generate();

    public static DeleteCatalogCommand GenerateDeleteCommand(Guid? id = null) =>
        CreateDeleteCatalogCommandFaker(id).Generate();

    public static List<CatalogEntity> GenerateCatalogs(int count, string email = "") =>
        CreateCatalogFaker().Generate(count);
}