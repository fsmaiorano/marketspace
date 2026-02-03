using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Payment.UpdatePayment;

public class UpdatePaymentCommand
{
    public Guid Id { get; set; }
    public PaymentStatusEnum Status { get; set; }
    public string? StatusDetail { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
}
