using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Entities;

public class PaymentTransactionEntity : Entity<PaymentTransactionId>
{
    public PaymentId PaymentId { get; private set; }

    public PaymentTransactionTypeEnum Type { get; private set; }
    public decimal Amount { get; private set; }

    public string? ProviderTransactionId { get; private set; }

    public PaymentTransactionEntity(PaymentId paymentId, PaymentTransactionTypeEnum type, decimal amount, string? providerTransactionId)
    {
        Id = PaymentTransactionId.Of(Guid.NewGuid());
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
        CreatedAt = DateTime.UtcNow;
    }
}
