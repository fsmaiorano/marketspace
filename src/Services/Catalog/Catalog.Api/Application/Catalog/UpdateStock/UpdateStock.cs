using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateStock;

public record UpdateStockCommand(Guid CatalogId, int Delta);

public record UpdateStockResult(int Available, int Reserved, Guid MerchantId, string ProductName);

public class UpdateStock(
    ICatalogRepository repository,
    IAppLogger<UpdateStock> logger)
{
    public async Task<Result<UpdateStockResult>> HandleAsync(UpdateStockCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing stock adjustment for catalog {CatalogId}, delta: {Delta}",
                command.CatalogId, command.Delta);

            CatalogEntity? entity = await repository.GetByIdAsync(
                CatalogId.Of(command.CatalogId), isTrackingEnabled: true);

            if (entity is null)
                return Result<UpdateStockResult>.Failure($"Catalog {command.CatalogId} not found.");

            entity.AdjustAvailableStock(command.Delta);
            await repository.UpdateAsync(entity);

            logger.LogInformation(LogTypeEnum.Business,
                "Stock adjusted for catalog {CatalogId}. Available: {Available}, Reserved: {Reserved}",
                command.CatalogId, entity.Stock.Available, entity.Stock.Reserved);

            return Result<UpdateStockResult>.Success(
                new UpdateStockResult(entity.Stock.Available, entity.Stock.Reserved, entity.MerchantId, entity.Name));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error adjusting stock.");
            return Result<UpdateStockResult>.Failure("An error occurred while processing your request.");
        }
    }
}
