using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Basket.Api.Domain.Repositories;

namespace Basket.Api.Application.Basket.DeleteBasket;

public class DeleteBasketHandler(
    IBasketDataRepository dataRepository, 
    IApplicationLogger<DeleteBasketHandler> applicationLogger,
    IBusinessLogger<DeleteBasketHandler> businessLogger)
    : IDeleteBasketHandler
{
    public async Task<Result<DeleteBasketResult>> HandleAsync(DeleteBasketCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing delete basket request for user: {Username}", command.Username);
            
            await dataRepository.DeleteCartAsync(command.Username);
            
            businessLogger.LogInformation("Basket deleted successfully. Username: {Username}", command.Username);
            return Result<DeleteBasketResult>.Success(new DeleteBasketResult(true));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while deleting the basket: {Command}", command);
            return Result<DeleteBasketResult>.Failure("An error occurred while deleting the basket.");
        }
    }
}