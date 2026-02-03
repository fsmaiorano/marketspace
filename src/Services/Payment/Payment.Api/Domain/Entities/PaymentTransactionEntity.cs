using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.Entities;

public class PaymentTransactionEntity
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }

    public PaymentTransactionTypeEnum Type { get; private set; }
    public decimal Amount { get; private set; }

    public string? ProviderTransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PaymentTransactionEntity() { }

    public PaymentTransactionEntity(Guid paymentId, PaymentTransactionTypeEnum type, decimal amount, string? providerTransactionId)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
        CreatedAt = DateTime.UtcNow;
    }
}
