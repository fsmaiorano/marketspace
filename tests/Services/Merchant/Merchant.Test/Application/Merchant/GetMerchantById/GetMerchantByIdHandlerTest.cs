using Merchant.Api.Application.Merchant.GetMerchantById;

namespace Merchant.Test.Application.Merchant.GetMerchantById;

public class GetMerchantByIdHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenMerchantExists()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<IAppLogger<GetMerchantByIdHandler>> loggerMock = new();
        
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<MerchantId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        GetMerchantByIdHandler handler = new(repositoryMock.Object, loggerMock.Object);
        GetMerchantByIdQuery query = new GetMerchantByIdQuery(merchant.Id.Value);

        Result<GetMerchantByIdResult> result = await handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}