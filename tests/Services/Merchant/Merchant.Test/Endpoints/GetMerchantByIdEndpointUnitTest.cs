using Builder;
using BuildingBlocks;
using FluentAssertions;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Api.Domain.Entities;
using Merchant.Test.Base;
using Merchant.Test.Fixtures;
using Moq;
using System.Net.Http.Json;

namespace Merchant.Test.Endpoints;

public class GetMerchantByIdEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    private readonly Mock<IGetMerchantByIdHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Merchant_Exists()
    {
        Guid merchantId = Guid.CreateVersion7();
        GetMerchantByIdQuery query = new GetMerchantByIdQuery(merchantId);
        GetMerchantByIdResult result = new GetMerchantByIdResult
        {
            Id = merchantId,
            Name = "Test Merchant",
            Email = "merchant@marketspace.com",
            PhoneNumber = "123456789",
            Address = "123 Market St, City, Country"
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetMerchantByIdQuery>()))
            .ReturnsAsync(Result<GetMerchantByIdResult>.Success(result));

        Result<GetMerchantByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_NotFound_When_Merchant_Does_Not_Exist()
    {
        Guid merchantId = Guid.CreateVersion7();
        GetMerchantByIdQuery query = new GetMerchantByIdQuery(merchantId);
        Result<GetMerchantByIdResult> result =
            Result<GetMerchantByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetMerchantByIdQuery>()))
            .ReturnsAsync(result);

        Result<GetMerchantByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be($"Catalog with ID {query.Id} not found.");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        GetMerchantByIdQuery query = new GetMerchantByIdQuery(Guid.CreateVersion7());
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetMerchantByIdQuery>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(query);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Get_Merchant_By_Id_Endpoint()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        Context.Merchants.Add(merchant);
        await Context.SaveChangesAsync();

        GetMerchantByIdQuery query = new(merchant.Id.Value);
        GetMerchantByIdResult result = new GetMerchantByIdResult()
        {
            Id = merchant.Id.Value,
            Name = merchant.Name,
            Email = merchant.Email.Value,
            PhoneNumber = merchant.PhoneNumber,
            Address = merchant.Address
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetMerchantByIdQuery>()))
            .ReturnsAsync(Result<GetMerchantByIdResult>.Success(result));

        HttpResponseMessage response = await DoGet($"/merchant/{merchant.Id.Value}");
        response.EnsureSuccessStatusCode();

        Result<GetMerchantByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResult>>();
        responseResult.Should().NotBeNull();
    }
}
