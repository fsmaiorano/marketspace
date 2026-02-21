using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

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

    public PaymentAttemptEntity()
    {
    }

    [JsonConstructor]
    public PaymentAttemptEntity(PaymentAttemptId id, PaymentId paymentId, int attemptNumber, 
        PaymentAttemptStatus status, string? statusDetail, string? providerTransactionId,
        string? responseCode, string? responseMessage)
    {
        Id = id;
        PaymentId = paymentId;
        AttemptNumber = attemptNumber;
        Status = status;
        StatusDetail = statusDetail;
        ProviderTransactionId = providerTransactionId;
        ResponseCode = responseCode;
        ResponseMessage = responseMessage;
    }

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
        Touch();
    }

    private void ChangeStatus(PaymentAttemptStatus status)
    {
        if (Status == status)
            return;

        Status = status;
    }

    private void ChangeStatusDetail(string? detail)
    {
        if (StatusDetail == detail)
            return;

        StatusDetail = detail;
    }

    private void ChangeProviderTransactionId(string? providerTransactionId)
    {
        if (ProviderTransactionId == providerTransactionId)
            return;

        ProviderTransactionId = providerTransactionId;
    }

    private void ChangeResponseCode(string? responseCode)
    {
        if (ResponseCode == responseCode)
            return;

        ResponseCode = responseCode;
    }

    private void ChangeResponseMessage(string? responseMessage)
    {
        if (ResponseMessage == responseMessage)
            return;

        ResponseMessage = responseMessage;
    }

    private void Touch() => LastModifiedAt = DateTime.UtcNow;

    public void Update(
        PaymentAttemptStatus? status = null,
        string? statusDetail = null,
        string? providerTransactionId = null,
        string? responseCode = null,
        string? responseMessage = null)
    {
        if (status is not null)
            ChangeStatus(status.Value);

        if (statusDetail is not null)
            ChangeStatusDetail(statusDetail);

        if (providerTransactionId is not null)
            ChangeProviderTransactionId(providerTransactionId);

        if (responseCode is not null)
            ChangeResponseCode(responseCode);

        if (responseMessage is not null)
            ChangeResponseMessage(responseMessage);

        Touch();
    }
}
