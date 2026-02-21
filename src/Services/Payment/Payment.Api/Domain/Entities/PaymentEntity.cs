using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Events;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

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

    public PaymentEntity()
    {
    }

    [JsonConstructor]
    public PaymentEntity(PaymentId id, Guid orderId, decimal amount, string currency, 
        PaymentStatusEnum status, string? statusDetail, PaymentMethod method, string provider,
        string? providerTransactionId, string? authorizationCode,
        ICollection<PaymentAttemptEntity>? attempts, ICollection<PaymentTransactionEntity>? transactions,
        RiskAnalysisEntity? riskAnalysis)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        Status = status;
        StatusDetail = statusDetail;
        Method = method;
        Provider = provider;
        ProviderTransactionId = providerTransactionId;
        AuthorizationCode = authorizationCode;
        Attempts = attempts ?? [];
        Transactions = transactions ?? [];
        RiskAnalysis = riskAnalysis;
    }

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

    private void ChangeAmount(decimal amount)
    {
        if (Amount == amount)
            return;

        Amount = amount;
    }

    private void ChangeCurrency(string currency)
    {
        ArgumentNullException.ThrowIfNull(currency, nameof(currency));

        if (Currency == currency)
            return;

        Currency = currency;
    }

    private void ChangeMethod(PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));

        if (Method == method)
            return;

        Method = method;
    }

    private void ChangeProvider(string provider)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));

        if (Provider == provider)
            return;

        Provider = provider;
    }

    private void ChangeProviderTransactionId(string? providerTransactionId)
    {
        if (ProviderTransactionId == providerTransactionId)
            return;

        ProviderTransactionId = providerTransactionId;
    }

    private void ChangeAuthorizationCode(string? authorizationCode)
    {
        if (AuthorizationCode == authorizationCode)
            return;

        AuthorizationCode = authorizationCode;
    }

    private void ChangeStatus(PaymentStatusEnum status, bool addEvent = true)
    {
        if (Status == status)
            return;

        Status = status;

        if (addEvent)
            AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    private void ChangeStatusDetail(string? detail)
    {
        if (StatusDetail == detail)
            return;

        StatusDetail = detail;
    }

    private void Touch() => LastModifiedAt = DateTime.UtcNow;

    public void Update(
        decimal? amount = null,
        string? currency = null,
        PaymentMethod? method = null,
        string? provider = null,
        string? providerTransactionId = null,
        string? authorizationCode = null,
        PaymentStatusEnum? status = null,
        string? statusDetail = null)
    {
        if (amount is not null)
            ChangeAmount(amount.Value);

        if (currency is not null)
            ChangeCurrency(currency);

        if (method is not null)
            ChangeMethod(method);

        if (provider is not null)
            ChangeProvider(provider);

        if (providerTransactionId is not null)
            ChangeProviderTransactionId(providerTransactionId);

        if (authorizationCode is not null)
            ChangeAuthorizationCode(authorizationCode);

        if (status is not null)
            ChangeStatus(status.Value);

        if (statusDetail is not null)
            ChangeStatusDetail(statusDetail);

        Touch();
    }

    public void PatchStatus(PaymentStatusEnum status)
    {
        ChangeStatus(status);
        Touch();
    }

    public void MarkCreated()
    {
        ChangeStatus(PaymentStatusEnum.Created);
        Touch();
    }

    public void MarkProcessing()
    {
        ChangeStatus(PaymentStatusEnum.Processing);
        Touch();
    }

    public void MarkAuthorized()
    {
        ChangeStatus(PaymentStatusEnum.Authorized);
        Touch();
    }

    public void MarkCaptured()
    {
        ChangeStatus(PaymentStatusEnum.Captured);
        Touch();
    }

    public void MarkRefunded()
    {
        ChangeStatus(PaymentStatusEnum.Refunded);
        Touch();
    }

    public void MarkCancelled()
    {
        ChangeStatus(PaymentStatusEnum.Cancelled);
        Touch();
    }

    public void MarkFailed()
    {
        ChangeStatus(PaymentStatusEnum.Failed);
        Touch();
    }

    public void Approve(string transactionId, string? authCode)
    {
        ChangeStatus(PaymentStatusEnum.Authorized, addEvent: false);
        ChangeProviderTransactionId(transactionId);
        ChangeAuthorizationCode(authCode);
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void Fail(string detail)
    {
        ChangeStatus(PaymentStatusEnum.Failed, addEvent: false);
        ChangeStatusDetail(detail);
        Touch();
        AddDomainEvent(new PaymentStatusChangedDomainEvent(this));
    }

    public void AddAttempt(PaymentAttemptEntity attempt)
        => Attempts.Add(attempt);

    public void AddTransaction(PaymentTransactionEntity transaction)
        => Transactions.Add(transaction);

    public void SetRisk(RiskAnalysisEntity risk)
        => RiskAnalysis = risk;
}