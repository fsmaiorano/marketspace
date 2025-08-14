namespace Merchant.Test.Application.Merchant.UpdateMerchant;

public class UpdateMerchantHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<ILogger<UpdateMerchantHandler>> loggerMock = new();

        MerchantId merchantId = MerchantId.Of(Guid.NewGuid());

        // repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<MerchantEntity>()))
        //     .Returns((MerchantEntity m) =>
        //     {
        //         m.Id = merchantId;
        //         return Task.FromResult(m);
        //     });
        
        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<MerchantEntity>()))
            .ReturnsAsync(1);

        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<MerchantId>()))
            .ReturnsAsync(() => merchant);

        UpdateMerchantHandler handler = new UpdateMerchantHandler(repositoryMock.Object, loggerMock.Object);

        UpdateMerchantCommand command = new UpdateMerchantCommand
        {
            Id = merchantId.Value,
            Name = $"{merchant.Name}_Updated",
            Email = merchant.Email.Value,
            PhoneNumber = merchant.PhoneNumber,
            Address = merchant.Address
        };

        Result<UpdateMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}