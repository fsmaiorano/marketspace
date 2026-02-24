using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace Basket.Api.Application.Basket.GetBasketById;

public record GetBasketByIdQuery(string Username);

public record GetBasketByIdResult(ShoppingCartEntity? ShoppingCart);

public class GetBasketById(
    IBasketDataRepository basketRepository,
    IAppLogger<GetBasketById> logger)
{
    public async Task<Result<GetBasketByIdResult>> HandleAsync(GetBasketByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get basket request for user: {Username}",
                query.Username);

            ShoppingCartEntity? shoppingCart = await basketRepository.GetCartAsync(query.Username);

            return shoppingCart is null
                ? Result<GetBasketByIdResult>.Failure("No basket found for the specified username.")
                : Result<GetBasketByIdResult>.Success(new GetBasketByIdResult(shoppingCart));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting basket by username.");
            return Result<GetBasketByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}