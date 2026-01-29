using Builder;
using BuildingBlocks;
using FluentAssertions;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Test.Base;
using Merchant.Test.Fixtures;
using Moq;
using System.Net.Http.Json;

namespace Merchant.Test.Endpoints;

public class CreateMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly Mock<ICreateMerchantHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/merchant", command);
        Result<CreateMerchantResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateMerchantResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}