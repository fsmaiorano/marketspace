using BuildingBlocks;
using BuildingBlocks.Loggers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Application.Catalog.ReserveStock;

public record ReserveStockCommand(Guid CatalogId, int Quantity);

public record ReserveStockResult(int Available, int Reserved, Guid MerchantId, string ProductName);

public class ReserveStock(
    ICatalogRepository repository,
    IAppLogger<ReserveStock> logger)
{
    private const int MaxRetries = 3;

    public async Task<Result<ReserveStockResult>> HandleAsync(ReserveStockCommand command,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                logger.LogInformation(LogTypeEnum.Application,
                    "Reserving {Quantity} unit(s) for catalog {CatalogId} (attempt {Attempt})",
                    command.Quantity, command.CatalogId, attempt);

                CatalogEntity? entity = await repository.GetByIdAsync(
                    CatalogId.Of(command.CatalogId), isTrackingEnabled: true);

                if (entity is null)
                    return Result<ReserveStockResult>.Failure($"Catalog {command.CatalogId} not found.");

                if (entity.Stock.Available < command.Quantity)
                    return Result<ReserveStockResult>.Failure(
                        $"Insufficient stock. Available: {entity.Stock.Available}, Requested: {command.Quantity}.");

                entity.ReserveStock(command.Quantity);
                await repository.UpdateAsync(entity);

                logger.LogInformation(LogTypeEnum.Business,
                    "Stock reserved for catalog {CatalogId}. Available: {Available}, Reserved: {Reserved}",
                    command.CatalogId, entity.Stock.Available, entity.Stock.Reserved);

                return Result<ReserveStockResult>.Success(
                    new ReserveStockResult(entity.Stock.Available, entity.Stock.Reserved, entity.MerchantId, entity.Name));
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Concurrency conflict reserving stock for catalog {CatalogId}. Retrying (attempt {Attempt})...",
                    command.CatalogId, attempt);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError(LogTypeEnum.Exception, null,
                    "Concurrency conflict reserving stock for catalog {CatalogId} after {MaxRetries} attempts.",
                    command.CatalogId, MaxRetries);

                return Result<ReserveStockResult>.Failure("Concurrency conflict. Please retry.");
            }
            catch (Exception ex)
            {
                logger.LogError(LogTypeEnum.Exception, ex, "Error reserving stock.");
                return Result<ReserveStockResult>.Failure("An error occurred while processing your request.");
            }
        }

        return Result<ReserveStockResult>.Failure("Failed to reserve stock after maximum retries.");
    }
}
