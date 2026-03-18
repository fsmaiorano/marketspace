using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Endpoints.Dto;

namespace Order.Api.Application.Order.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(Guid CustomerId, int Limit = 10);

public record GetOrdersByCustomerResult(List<OrderEntity> Orders);

public class GetOrdersByCustomer(
    IOrderRepository repository,
    IAppLogger<GetOrdersByCustomer> logger)
{
    public async Task<Result<GetOrdersByCustomerResult>> HandleAsync(GetOrdersByCustomerQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Fetching orders for customer {CustomerId}", query.CustomerId);

            List<OrderEntity> orders =
                await repository.GetByCustomerIdAsync(query.CustomerId, query.Limit);

            return Result<GetOrdersByCustomerResult>.Success(new GetOrdersByCustomerResult(orders));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error fetching orders for customer.");
            return Result<GetOrdersByCustomerResult>.Failure("An error occurred while processing your request.");
        }
    }
}
