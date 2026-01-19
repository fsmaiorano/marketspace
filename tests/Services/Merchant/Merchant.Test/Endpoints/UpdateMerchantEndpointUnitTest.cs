using Builder;
using BuildingBlocks;
using FluentAssertions;
using Merchant.Api.Application.Merchant.UpdateMerchant;
using Merchant.Api.Domain.Entities;
using Merchant.Test.Base;
using Merchant.Test.Fixtures;
using Moq;
using System.Net.Http.Json;

namespace Merchant.Test.Endpoints;

public class UpdateMerchantEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    private readonly Mock<IUpdateMerchantHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Failure_When_Merchant_Not_Found()
    {
        Guid merchantId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMerchantCommand>()))
            .ReturnsAsync(Result<UpdateMerchantResult>.Failure("Catalog not found."));

        UpdateMerchantCommand command = new UpdateMerchantCommand { Id = merchantId };

        Result<UpdateMerchantResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Catalog not found.");
    }

    [Fact]
    public async Task Returns_Failure_When_Exception_Occurs()
    {
        Guid merchantId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMerchantCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        UpdateMerchantCommand command = new UpdateMerchantCommand { Id = merchantId };

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Returns_Success_When_Merchant_Updated()
    {
        Guid merchantId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMerchantCommand>()))
            .ReturnsAsync(Result<UpdateMerchantResult>.Success(new UpdateMerchantResult(isSuccess: true)));

        UpdateMerchantCommand command = new UpdateMerchantCommand { Id = merchantId };

        Result<UpdateMerchantResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data?.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Update_Merchant_Endpoint()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();

        Context.Merchants.Add(merchant);
        await Context.SaveChangesAsync();

        UpdateMerchantCommand command = new UpdateMerchantCommand
        {
            Id = merchant.Id.Value,
            Name = "Updated Catalog Name",
            Description = "Updated Description",
            Email = merchant.Email.Value,
            Address = merchant.Address,
            PhoneNumber = merchant.PhoneNumber
        };

        HttpResponseMessage response = await DoPut("/merchant", command);

        UpdateMerchantResult? result = await response.Content.ReadFromJsonAsync<UpdateMerchantResult>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }
}
