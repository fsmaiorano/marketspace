using BuildingBlocks;
using BuildingBlocks.Loggers;
using Basket.Api.Domain.Repositories;

namespace Basket.Api.Application.Basket.DeleteBasket;

public record DeleteBasketCommand(string Username);

public record DeleteBasketResult();

public class DeleteBasket(
    IBasketDataRepository dataRepository,
    IAppLogger<DeleteBasket> logger)
{
    public async Task<Result<DeleteBasketResult>> HandleAsync(DeleteBasketCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing delete basket request for user: {Username}",
                command.Username);

            await dataRepository.DeleteCartAsync(command.Username);

            logger.LogInformation(LogTypeEnum.Business, "Basket deleted successfully. Username: {Username}",
                command.Username);
            
            return Result<DeleteBasketResult>.Success(new DeleteBasketResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while deleting the basket: {Command}",
                command);
            return Result<DeleteBasketResult>.Failure("An error occurred while deleting the basket.");
        }
    }
}