using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Payment.Api.Domain.Entities;

public class PaymentTransactionEntity : Entity<PaymentTransactionId>
{
    public PaymentId PaymentId { get; private set; }

    public PaymentTransactionTypeEnum Type { get; private set; }
    public decimal Amount { get; private set; }

    public string? ProviderTransactionId { get; private set; }

    public PaymentTransactionEntity()
    {
    }

    [JsonConstructor]
    public PaymentTransactionEntity(PaymentTransactionId id, PaymentId paymentId, 
        PaymentTransactionTypeEnum type, decimal amount, string? providerTransactionId)
    {
        Id = id;
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
    }

    public PaymentTransactionEntity(PaymentId paymentId, PaymentTransactionTypeEnum type, decimal amount, string? providerTransactionId)
    {
        Id = PaymentTransactionId.Of(Guid.NewGuid());
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
        CreatedAt = DateTime.UtcNow;
    }

    private void ChangeType(PaymentTransactionTypeEnum type)
    {
        if (Type == type)
            return;

        Type = type;
    }

    private void ChangeAmount(decimal amount)
    {
        if (Amount == amount)
            return;

        Amount = amount;
    }

    private void ChangeProviderTransactionId(string? providerTransactionId)
    {
        if (ProviderTransactionId == providerTransactionId)
            return;

        ProviderTransactionId = providerTransactionId;
    }

    private void Touch() => LastModifiedAt = DateTime.UtcNow;

    public void Update(
        PaymentTransactionTypeEnum? type = null,
        decimal? amount = null,
        string? providerTransactionId = null)
    {
        if (type is not null)
            ChangeType(type.Value);

        if (amount is not null)
            ChangeAmount(amount.Value);

        if (providerTransactionId is not null)
            ChangeProviderTransactionId(providerTransactionId);

        Touch();
    }
}
