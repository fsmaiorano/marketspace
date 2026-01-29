using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Test.Base;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class UpdateCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly Mock<IUpdateCatalogHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Success_When_Catalog_Is_Updated()
    {
        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();
        catalog.Id = CatalogId.Of(Guid.NewGuid());

        Context.Catalogs.Add(catalog);
        await Context.SaveChangesAsync();

        UpdateCatalogCommand command = new()
        {
            Id = catalog.Id.Value,
            Name = "Updated Catalog Name",
            Description = catalog.Description,
            ImageUrl = catalog.ImageUrl,
            Price = catalog.Price.Value,
            Categories = catalog.Categories,
            MerchantId = catalog.MerchantId
        };

        HttpResponseMessage response = await DoPut("/catalog", command);
        Result<UpdateCatalogResult>? result = await response.Content.ReadFromJsonAsync<Result<UpdateCatalogResult>>();

        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();

        Context.ChangeTracker.Clear();
        CatalogEntity? updatedCatalog = await Context.Catalogs.FindAsync(catalog.Id);
        updatedCatalog.Should().NotBeNull();
        updatedCatalog!.Name.Should().Be("Updated Catalog Name");
    }
}