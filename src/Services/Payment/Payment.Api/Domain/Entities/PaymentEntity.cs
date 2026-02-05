using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Events;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Entities;

public class PaymentEntity : Aggregate<PaymentId>
{
    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public PaymentStatusEnum Status { get; private set; }
    public string? StatusDetail { get; private set; }

    public PaymentMethod Method { get; private set; }
    public string Provider { get; private set; }

    public string? ProviderTransactionId { get; private set; }
    public string? AuthorizationCode { get; private set; }

    // NAVIGATION
    public ICollection<PaymentAttemptEntity> Attempts { get; private set; } = [];

    public ICollection<PaymentTransactionEntity> Transactions { get; private set; } = [];

    public RiskAnalysisEntity? RiskAnalysis { get; private set; }

    public static PaymentEntity Create(Guid orderId, decimal amount, string currency, PaymentMethod method,
        string provider)
    {
        ArgumentNullException.ThrowIfNull(currency, nameof(currency));
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));

        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        PaymentEntity payment = new()
        {
            Id = PaymentId.Of(Guid.NewGuid()),
            OrderId = orderId,
            Amount = amount,
            Currency = currency,
            Method = method,
            Provider = provider,
            Status = PaymentStatusEnum.Created,
            CreatedAt = DateTime.UtcNow
        };

        return payment;
    }

    public static PaymentEntity Update(PaymentEntity existingPayment, decimal amount, string currency,
        PaymentMethod method, string provider)
    {
        ArgumentNullException.ThrowIfNull(existingPayment, nameof(existingPayment));
        ArgumentNullException.ThrowIfNull(currency, nameof(currency));
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));

        existingPayment.Amount = amount;
        existingPayment.Currency = currency;
        existingPayment.Method = method;
        existingPayment.Provider = provider;
        existingPayment.Touch();

        existingPayment.AddDomainEvent(new PaymentStatusChangedDomainEvent(existingPayment));

        return existingPayment;
    }

    public void PatchStatus(PaymentStatusEnum status)
    {
        Status = status;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkCreated()
    {
        Status = PaymentStatusEnum.Created;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkProcessing()
    {
        Status = PaymentStatusEnum.Processing;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkAuthorized()
    {
        Status = PaymentStatusEnum.Authorized;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkCaptured()
    {
        Status = PaymentStatusEnum.Captured;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkRefunded()
    {
        Status = PaymentStatusEnum.Refunded;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkCancelled()
    {
        Status = PaymentStatusEnum.Cancelled;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void MarkFailed()
    {
        Status = PaymentStatusEnum.Failed;
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void Approve(string transactionId, string? authCode)
    {
        Status = PaymentStatusEnum.Authorized;
        ProviderTransactionId = transactionId;
        AuthorizationCode = authCode;
        Touch();
    }

    public void Fail(string detail)
    {
        Status = PaymentStatusEnum.Failed;
        StatusDetail = detail;
        Touch();
    }

    public void AddAttempt(PaymentAttemptEntity attempt)
        => Attempts.Add(attempt);

    public void AddTransaction(PaymentTransactionEntity transaction)
        => Transactions.Add(transaction);

    public void SetRisk(RiskAnalysisEntity risk)
        => RiskAnalysis = risk;

    private void Touch() => LastModifiedAt = DateTime.UtcNow;
}