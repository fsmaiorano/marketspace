using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Payment.CreatePayment;

public class CreatePaymentCommand
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Method { get; init; }
    public string Provider { get; init; } = string.Empty;
}
