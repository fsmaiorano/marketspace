using Basket.Api.Domain.Entities;

namespace Basket.Test.Application.Basket.CreateBasket;

public class CreateBasketHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        Mock<IBasketDataRepository> repositoryMock = new();
        Mock<IApplicationLogger<CreateBasketHandler>> applicationLoggerMock = new();
        Mock<IBusinessLogger<CreateBasketHandler>> businessLoggerMock = new();
        ShoppingCartEntity? shoppingCartEntity = BasketBuilder.CreateShoppingCartFaker("").Generate();

        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker();

        CreateBasketHandler handler = new CreateBasketHandler(
            repositoryMock.Object, 
            applicationLoggerMock.Object, 
            businessLoggerMock.Object);
        
        Result<CreateBasketResult> result = await handler.HandleAsync(command);




    }
}