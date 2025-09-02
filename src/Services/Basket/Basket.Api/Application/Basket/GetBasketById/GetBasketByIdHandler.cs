using Basket.Api.Application.Dto;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;

namespace Basket.Api.Application.Basket.GetBasketById;

public class GetBasketByIdHandler(IBasketRepository repository, ILogger<GetBasketByIdHandler> logger)
    : IGetBasketByIdHandler
{
    public async Task<Result<GetBasketByIdResult>> HandleAsync(GetBasketByIdQuery query)
    {
        try
        {
            ShoppingCartEntity? shoppingCart = await repository.GetCartAsync(query.Username);

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
            logger.LogError(ex, "An error occurred while getting basket by username.");
            return Result<GetBasketByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}