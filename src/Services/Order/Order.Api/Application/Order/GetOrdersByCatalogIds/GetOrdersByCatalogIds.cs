using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;

namespace Order.Api.Application.Order.GetOrdersByCatalogIds;

public record GetOrdersByCatalogIdsQuery(IReadOnlyCollection<Guid> CatalogIds, int Limit = 50);

public record GetOrdersByCatalogIdsResult(IReadOnlyCollection<OrderEntity> Orders);

public class GetOrdersByCatalogIds(
    IOrderRepository repository,
    IAppLogger<GetOrdersByCatalogIds> logger)
{
    public async Task<Result<GetOrdersByCatalogIdsResult>> HandleAsync(GetOrdersByCatalogIdsQuery query)
    {
        try
        {
            if (query.CatalogIds.Count == 0)
                return Result<GetOrdersByCatalogIdsResult>.Success(new GetOrdersByCatalogIdsResult([]));

            List<OrderEntity> orders = await repository.GetRecentByCatalogIdsAsync(
                query.CatalogIds,
                query.Limit <= 0 ? 50 : query.Limit);

            logger.LogInformation(
                LogTypeEnum.Application,
                "Retrieved {OrderCount} recent orders for {CatalogCount} catalog IDs",
                orders.Count,
                query.CatalogIds.Count);

            return Result<GetOrdersByCatalogIdsResult>.Success(new GetOrdersByCatalogIdsResult(orders));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting orders by catalog IDs.");
            return Result<GetOrdersByCatalogIdsResult>.Failure("An error occurred while processing your request.");
        }
    }
}
