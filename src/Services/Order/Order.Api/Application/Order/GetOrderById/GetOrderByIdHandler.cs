using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Order.Api.Application.Order.GetOrderById;

public class GetOrderByIdHandler(
    IOrderRepository repository, 
    IApplicationLogger<GetOrderByIdHandler> applicationLogger,
    IBusinessLogger<GetOrderByIdHandler> businessLogger)
    : IGetOrderByIdHandler
{
    public async Task<Result<GetOrderByIdResult>> HandleAsync(GetOrderByIdQuery query)
    {
        try
        {
            applicationLogger.LogInformation("Processing get order by ID request for: {OrderId}", query.Id);
            
            OrderId catalogId = OrderId.Of(query.Id);

            OrderEntity? catalog = await repository.GetByIdAsync(catalogId, isTrackingEnabled: false);

            if (catalog is null)
                return Result<GetOrderByIdResult>.Failure($"Order with ID {query.Id} not found.");

            GetOrderByIdResult result = new GetOrderByIdResult { Id = catalog.Id.Value, };

            return Result<GetOrderByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while getting order by ID.");
            return Result<GetOrderByIdResult>.Failure("An error occurred while processing your request.");
        }
    }
}