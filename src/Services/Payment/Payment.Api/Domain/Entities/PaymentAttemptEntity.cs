using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Entities;

public class PaymentAttemptEntity : Entity<PaymentAttemptId>
{
    public PaymentId PaymentId { get; private set; }

    public int AttemptNumber { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string? StatusDetail { get; private set; }

    public string? ProviderTransactionId { get; private set; }
    public string? ResponseCode { get; private set; }
    public string? ResponseMessage { get; private set; }

    public PaymentAttemptEntity(PaymentId paymentId, int attemptNumber)
    {
        Id = PaymentAttemptId.Of(Guid.NewGuid());
        PaymentId = paymentId;
        AttemptNumber = attemptNumber;
        Status = PaymentAttemptStatus.Started;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete(PaymentAttemptStatus status, string? detail)
    {
        Status = status;
        StatusDetail = detail;
    }
}
