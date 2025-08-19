using BuildingBlocks;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Order.Api.Application.Order.GetOrderById;

public class GetOrderByIdHandler(IOrderRepository repository, ILogger<GetOrderByIdHandler> logger)
    : IGetOrderByIdHandler
{
    public async Task<Result<GetOrderByIdResult>> HandleAsync(GetOrderByIdQuery query)
    {
        try
        {
            OrderId catalogId = OrderId.Of(query.Id);

            OrderEntity? catalog = await repository.GetByIdAsync(catalogId, isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetOrderByIdResult>.Failure($"Order with ID {query.Id} not found.");

            GetOrderByIdResult result = new GetOrderByIdResult { Id = catalog.Id.Value, };

            return Result<GetOrderByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting catalog by ID.");
            return Result<GetOrderByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}