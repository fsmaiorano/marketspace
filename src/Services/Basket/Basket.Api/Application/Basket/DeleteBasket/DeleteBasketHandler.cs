using BuildingBlocks;
using Basket.Api.Domain.Repositories;

namespace Basket.Api.Application.Basket.DeleteBasket;

public class DeleteBasketHandler(IBasketRepository repository, ILogger<DeleteBasketHandler> logger)
    : IDeleteBasketHandler
{
    public async Task<Result<DeleteBasketResult>> HandleAsync(DeleteBasketCommand command)
    {
        try
        {
            await repository.DeleteCartAsync(command.Username);
            logger.LogInformation("Basket deleted successfully: {Username}", command.Username);
            return Result<DeleteBasketResult>.Success(new DeleteBasketResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the basket: {Command}", command);
            return Result<DeleteBasketResult>.Failure("An error occurred while deleting the basket.");
        }
    }
}