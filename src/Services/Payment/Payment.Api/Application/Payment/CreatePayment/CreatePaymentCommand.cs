using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Payment.CreatePayment;

public class CreatePaymentCommand
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
