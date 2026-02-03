using Payment.Api.Application.Dto;

namespace Payment.Api.Application.Payment.GetPaymentById;

public class GetPaymentByIdResult
{
    public PaymentDto Payment { get; init; } = null!;
}
