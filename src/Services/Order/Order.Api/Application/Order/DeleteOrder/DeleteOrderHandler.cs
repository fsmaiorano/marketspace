using BuildingBlocks;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.DeleteOrder;

public class DeleteOrderHandler(IOrderRepository repository, ILogger<DeleteOrderHandler> logger)
    : IDeleteOrderHandler
{
    public async Task<Result<DeleteOrderResult>> HandleAsync(DeleteOrderCommand command)
    {
        try
        {
            OrderId orderId = OrderId.Of(command.Id);

            await repository.RemoveAsync(orderId);
            logger.LogInformation("Order deleted successfully: {OrderId}", command.Id);
            return Result<DeleteOrderResult>.Success(new DeleteOrderResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the order: {Command}", command);
            return Result<DeleteOrderResult>.Failure("An error occurred while deleting the order.");
        }
    }
}