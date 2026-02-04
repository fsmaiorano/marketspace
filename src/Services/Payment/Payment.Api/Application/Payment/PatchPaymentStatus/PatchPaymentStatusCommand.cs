using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Payment.PatchPaymentStatus;

public record PatchPaymentStatusCommand
{
    public required Guid Id { get; set; }
    public required PaymentStatusEnum Status { get; set; }
}