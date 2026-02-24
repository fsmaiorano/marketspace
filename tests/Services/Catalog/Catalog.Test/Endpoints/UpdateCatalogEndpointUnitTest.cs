using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Test.Fixtures;
using System.Net;

namespace Catalog.Test.Endpoints;

public class UpdateCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Success_When_Catalog_Is_Updated()
    {
        CatalogEntity? catalog = await fixture.CreateCatalog();

        UpdateCatalogCommand command = new()
        {
            Id = catalog.Id.Value,
            Name = catalog.Name + "_updated",
            Description = catalog.Description,
            ImageUrl = catalog.ImageUrl,
            Price = catalog.Price.Value,
            Categories = catalog.Categories,
            MerchantId = catalog.MerchantId
        };

        HttpResponseMessage response = await DoPut("/catalog", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

        Context.ChangeTracker.Clear();
        CatalogEntity? updatedCatalog = await Context.Catalogs.FindAsync(catalog.Id);
        updatedCatalog.Should().NotBeNull();
        updatedCatalog!.Name.Should().Be(catalog.Name + "_updated");
    }
}