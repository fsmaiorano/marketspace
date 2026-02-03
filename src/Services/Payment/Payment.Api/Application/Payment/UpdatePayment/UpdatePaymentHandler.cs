using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.UpdatePayment;

public sealed class UpdatePaymentHandler(
    IPaymentRepository repository,
    IAppLogger<UpdatePaymentHandler> logger
) : IUpdatePaymentHandler
{
    public async Task<Result<UpdatePaymentResult>> HandleAsync(UpdatePaymentCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing update payment request for Payment: {PaymentId}", command.Id);

            PaymentId paymentId = PaymentId.Of(command.Id);
            PaymentEntity? payment = await repository.GetByIdAsync(paymentId);

            if (payment == null)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Payment not found for update: {PaymentId}", command.Id);
                return Result<UpdatePaymentResult>.Failure("Payment not found.");
            }

            switch (command.Status)
            {
                case PaymentStatusEnum.Authorized when string.IsNullOrEmpty(command.ProviderTransactionId):
                    return Result<UpdatePaymentResult>.Failure("ProviderTransactionId is required for Authorization.");
                case PaymentStatusEnum.Authorized:
                    payment.Approve(command.ProviderTransactionId, command.AuthorizationCode);
                    break;
                case PaymentStatusEnum.Failed:
                    payment.Fail(command.StatusDetail ?? "Unknown failure");
                    break;
                case PaymentStatusEnum.Processing:
                    payment.MarkProcessing();
                    break;
                default:
                    break;
            }

            int result = await repository.UpdateAsync(payment);

            if (result <= 0)
            {
                logger.LogError(LogTypeEnum.Application, null,
                    "Failed to update payment: {PaymentId}", command.Id);
                return Result<UpdatePaymentResult>.Failure("Failed to update payment.");
            }

            logger.LogInformation(LogTypeEnum.Business,
                "Payment updated successfully. PaymentId: {PaymentId}, Status: {Status}",
                payment.Id.Value, payment.Status);

            return Result<UpdatePaymentResult>.Success(new UpdatePaymentResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex,
                "Error updating payment: {PaymentId}", command.Id);
            throw;
        }
    }
}