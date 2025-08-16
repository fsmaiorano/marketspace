using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Catalog.Test.Api.Endpoints;

public class UpdateCatalogEndpointTest(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IUpdateCatalogHandler> _mockHandler = new();
    private readonly CatalogApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Failure_When_Catalog_Not_Found()
    {
        Guid merchantId = Guid.NewGuid();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateCatalogCommand>()))
            .ReturnsAsync(Result<UpdateCatalogResult>.Failure("Catalog not found."));

        UpdateCatalogCommand command = new UpdateCatalogCommand { Id = merchantId };

        Result<UpdateCatalogResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Catalog not found.");
    }

    [Fact]
    public async Task Returns_Failure_When_Exception_Occurs()
    {
        Guid merchantId = Guid.NewGuid();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateCatalogCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        UpdateCatalogCommand command = new UpdateCatalogCommand { Id = merchantId };

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Returns_Success_When_Catalog_Updated()
    {
        Guid merchantId = Guid.NewGuid();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateCatalogCommand>()))
            .ReturnsAsync(Result<UpdateCatalogResult>.Success(new UpdateCatalogResult(isSuccess: true)));

        UpdateCatalogCommand command = new UpdateCatalogCommand { Id = merchantId };

        Result<UpdateCatalogResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Value?.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Update_Catalog_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? merchant = CatalogBuilder.CreateCatalogFaker().Generate();

        dbContext.Catalogs.Add(merchant);
        await dbContext.SaveChangesAsync();

        UpdateCatalogCommand command = new UpdateCatalogCommand
        {
            Id = merchant.Id.Value, Name = "Updated Catalog Name", Description = "Updated Description",
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/merchant",
            command);

        UpdateCatalogResult? result = await response.Content.ReadFromJsonAsync<UpdateCatalogResult>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }
}