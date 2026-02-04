using Order.Api.Domain.Enums;

namespace Order.Api.Application.Order.PatchOrderStatus;

public record PatchOrderStatusCommand
{
    public required Guid Id { get; set; }
    public required OrderStatusEnum Status { get; set; }
}
