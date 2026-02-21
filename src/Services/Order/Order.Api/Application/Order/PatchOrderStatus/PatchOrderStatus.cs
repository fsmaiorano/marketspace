using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.PatchOrderStatus;

public record PatchOrderStatusCommand
{
    public required Guid Id { get; init; }
    public required OrderStatusEnum Status { get; init; }
}

public record PatchOrderStatusResult();

public sealed class PatchOrderStatus(
    IOrderRepository repository,
    IAppLogger<PatchOrderStatus> logger)
{
    public async Task<Result<PatchOrderStatusResult>> HandleAsync(PatchOrderStatusCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing patch order status request for: {OrderId}",
                command.Status);

            OrderId orderId = OrderId.Of(command.Id);
            OrderEntity? orderEntity =
                await repository.GetByIdAsync(orderId, isTrackingEnabled: true, CancellationToken.None);

            if (orderEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Order not found for patching status: {OrderId}",
                    command.Id);
                return Result<PatchOrderStatusResult>.Failure($"Order with ID {command.Id} not found.");
            }

            if (!Enum.IsDefined(typeof(OrderStatusEnum), command.Status))
            {
                logger.LogWarning(LogTypeEnum.Application, "Invalid order status value: {Status}", command.Status);
                return Result<PatchOrderStatusResult>.Failure($"Invalid order status value: {command.Status}");
            }

            orderEntity.PatchStatus(command.Status);
            int result = await repository.UpdateAsync(orderEntity);

            return result <= 0
                ? Result<PatchOrderStatusResult>.Failure("Failed to update order status.")
                : Result<PatchOrderStatusResult>.Success(new PatchOrderStatusResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while patching the order status: {Command}",
                command);
            return Result<PatchOrderStatusResult>.Failure("An error occurred while patching the order status.");
        }
    }
}