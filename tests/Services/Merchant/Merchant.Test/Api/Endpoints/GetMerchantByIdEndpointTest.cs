using Merchant.Api.Application.Merchant.GetMerchantById;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Merchant.Test.Api.Endpoints;

public class GetMerchantByIdEndpointTest(MerchantApiFactory factory) : IClassFixture<MerchantApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IGetMerchantByIdHandler> _mockHandler = new();
    private readonly MerchantApiFactory _factory = factory;

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
        using IServiceScope scope = _factory.Services.CreateScope();
        MerchantDbContext dbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        dbContext.Merchants.Add(merchant);
        await dbContext.SaveChangesAsync();

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

        HttpRequestMessage request = new(HttpMethod.Get, $"/merchant/{merchant.Id.Value}");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Result<GetMerchantByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResult>>();
        responseResult.Should().NotBeNull();
    }
}