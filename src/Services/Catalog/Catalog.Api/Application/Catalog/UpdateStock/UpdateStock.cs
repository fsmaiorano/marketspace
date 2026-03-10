using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Catalog.UpdateStock;

public record UpdateStockCommand(Guid CatalogId, int Delta);

public record UpdateStockResult(int NewStock, Guid MerchantId, string ProductName);

public class UpdateStock(
    ICatalogRepository repository,
    IAppLogger<UpdateStock> logger)
{
    public async Task<Result<UpdateStockResult>> HandleAsync(UpdateStockCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing update stock request for catalog: {CatalogId}, delta: {Delta}",
                command.CatalogId, command.Delta);

            CatalogEntity? entity = await repository.GetByIdAsync(CatalogId.Of(command.CatalogId), isTrackingEnabled: true);

            if (entity is null)
                return Result<UpdateStockResult>.Failure($"Catalog with ID {command.CatalogId} not found.");

            int newStock = entity.Stock.Value + command.Delta;

            if (newStock < 0)
                return Result<UpdateStockResult>.Failure($"Insufficient stock. Current stock: {entity.Stock.Value}, delta: {command.Delta}.");

            entity.Update(name: null, categories: null, description: null, imageUrl: null, price: null, stock: Stock.Of(newStock));
            await repository.UpdateAsync(entity);

            logger.LogInformation(LogTypeEnum.Business, "Stock updated for catalog {CatalogId}. New stock: {NewStock}",
                command.CatalogId, newStock);

            return Result<UpdateStockResult>.Success(new UpdateStockResult(newStock, entity.MerchantId, entity.Name));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating stock.");
            return Result<UpdateStockResult>.Failure("An error occurred while processing your request.");
        }
    }
}
