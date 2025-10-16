using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.DeleteOrder;

public class DeleteOrderHandler(
    IOrderRepository repository, 
    IApplicationLogger<DeleteOrderHandler> applicationLogger,
    IBusinessLogger<DeleteOrderHandler> businessLogger)
    : IDeleteOrderHandler
{
    public async Task<Result<DeleteOrderResult>> HandleAsync(DeleteOrderCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing delete order request for: {OrderId}", command.Id);
            
            OrderId orderId = OrderId.Of(command.Id);

            await repository.RemoveAsync(orderId);
            
            businessLogger.LogInformation("Order deleted successfully. OrderId: {OrderId}", command.Id);
            return Result<DeleteOrderResult>.Success(new DeleteOrderResult(true));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while deleting the order: {Command}", command);
            return Result<DeleteOrderResult>.Failure("An error occurred while deleting the order.");
        }
    }
}