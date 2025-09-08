using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Catalog.Test.Api.Endpoints;

public class 
    DeleteCatalogEndpointTest(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IDeleteCatalogHandler> _mockHandler = new();
    private readonly CatalogApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Deleted_Successfully()
    {
        Guid catalogId = Guid.CreateVersion7();

        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker().Generate();
        Result<DeleteCatalogResult> result = Result<DeleteCatalogResult>.Success(new DeleteCatalogResult(true));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteCatalogCommand>()))
            .ReturnsAsync(result);

        Result<DeleteCatalogResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker().Generate();
        Result<DeleteCatalogResult> result = Result<DeleteCatalogResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteCatalogCommand>()))
            .ReturnsAsync(result);
        Result<DeleteCatalogResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteCatalogCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Delete_Catalog_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();
        
        catalog.Id = CatalogId.Of(Guid.CreateVersion7());

        dbContext.Catalogs.Add(catalog);
        await dbContext.SaveChangesAsync();

        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker(catalog.Id.Value).Generate();
        HttpRequestMessage request = new(HttpMethod.Delete, "/catalog") { Content = JsonContent.Create(command) };
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        DeleteCatalogResult? result = await response.Content.ReadFromJsonAsync<DeleteCatalogResult>();
        result.Should().NotBeNull();
    }
}