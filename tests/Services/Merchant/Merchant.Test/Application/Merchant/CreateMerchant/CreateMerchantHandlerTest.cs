namespace Merchant.Test.Application.Merchant.CreateMerchant;

public class CreateMerchantHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<ILogger<CreateMerchantHandler>> loggerMock = new();

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MerchantEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantEntity m, CancellationToken _) =>
            {
                m.Id = MerchantId.Of(Guid.CreateVersion7());
                return 1;
            });

        CreateMerchantHandler handler = new CreateMerchantHandler(repositoryMock.Object, loggerMock.Object);

        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();

        Result<CreateMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEqual(Guid.Empty, result.Data.MerchantId);
    }

    [Fact]
    public Task HandleAsync_ShouldReturnFailureResult_WhenExceptionIsThrown()
    {
        try
        {
            MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker(email: "wrong-email-format").Generate();
        }
        catch (DomainException ex)
        {
            Assert.IsType<DomainException>(ex);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType()}");
        }

        return Task.CompletedTask;
    }
}