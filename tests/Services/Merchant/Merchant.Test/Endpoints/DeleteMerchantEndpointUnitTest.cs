using Builder;
using BuildingBlocks;
using FluentAssertions;
using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Api.Domain.Entities;
using Merchant.Test.Base;
using Merchant.Test.Fixtures;
using Moq;
using System.Net.Http.Json;

namespace Merchant.Test.Endpoints;

public class DeleteMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly Mock<IDeleteMerchantHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Deleted_Successfully()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        Context.Merchants.Add(merchant);
        await Context.SaveChangesAsync();
        
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker(merchant.Id.Value).Generate();
        HttpResponseMessage response = await DoDelete($"/merchant/{command.Id}");
        Result<DeleteMerchantResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteMerchantResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}
