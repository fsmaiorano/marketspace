using Basket.Api.Application.Dto;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace Basket.Api.Application.Basket.GetBasketById;

public record GetBasketByIdQuery(string Username);

public class GetBasketByIdResult(ShoppingCartDto shoppingCart)
{
    public ShoppingCartDto ShoppingCart { get; init; } = shoppingCart;
};

public class GetBasketById(
    IBasketDataRepository dataRepository,
    IAppLogger<GetBasketById> logger)
{
    public async Task<Result<GetBasketByIdResult>> HandleAsync(GetBasketByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get basket request for user: {Username}",
                query.Username);

            ShoppingCartEntity? shoppingCart = await dataRepository.GetCartAsync(query.Username);

            if (shoppingCart is null)
            {
                ShoppingCartDto emptyDto = new() { Username = query.Username, Items = [] };
                GetBasketByIdResult emptyResult = new(emptyDto);
                return Result<GetBasketByIdResult>.Success(emptyResult);
            }

            ShoppingCartDto cartDto = new()
            {
                Username = shoppingCart.Username,
                Items = shoppingCart.Items.Select(item => new ShoppingCartItemDto
                {
                    Quantity = item.Quantity, Price = item.Price
                }).ToList()
            };

            GetBasketByIdResult result = new(cartDto);
            return Result<GetBasketByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting basket by username.");
            return Result<GetBasketByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}