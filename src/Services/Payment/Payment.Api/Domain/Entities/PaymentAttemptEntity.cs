using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.Entities;

public class PaymentAttemptEntity
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }

    public int AttemptNumber { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string? StatusDetail { get; private set; }

    public string? ProviderTransactionId { get; private set; }
    public string? ResponseCode { get; private set; }
    public string? ResponseMessage { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private PaymentAttemptEntity() { }
    public PaymentAttemptEntity(Guid paymentId, int attemptNumber)
    {
        Id = Guid.NewGuid();
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
