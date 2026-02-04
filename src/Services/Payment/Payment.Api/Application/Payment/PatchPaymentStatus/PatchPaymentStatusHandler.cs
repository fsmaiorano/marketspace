using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.PatchPaymentStatus;

public sealed class PatchPaymentStatusHandler(
    IPaymentRepository repository,
    IAppLogger<PatchPaymentStatusHandler> logger) : IPatchPaymentStatusHandler
{
    public async Task<Result<PatchPaymentStatusResult>> HandleAsync(PatchPaymentStatusCommand command)
    {
        try
        {
            logger.LogInformation("Handling PatchPaymentStatusCommand for Payment Id: {PaymentId}", command.Id);

            PaymentId paymentId = PaymentId.Of(command.Id);
            PaymentEntity? paymentEntity = await repository.GetByIdAsync(paymentId, true, CancellationToken.None);

            if (paymentEntity is null)
            {
                logger.LogWarning("Payment not found for patching status: {PaymentId}", command.Id);
                return Result<PatchPaymentStatusResult>.Failure($"Payment with ID {command.Id} not found.");
            }

            if (!Enum.IsDefined(typeof(PaymentStatusEnum), command.Status))
            {
                logger.LogWarning("Invalid payment status value: {Status}", command.Status);
                return Result<PatchPaymentStatusResult>.Failure($"Invalid payment status value: {command.Status}");
            }

            paymentEntity.PatchStatus(command.Status);
            await repository.UpdateAsync(paymentEntity);
            return Result<PatchPaymentStatusResult>.Success(new PatchPaymentStatusResult());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling PatchPaymentStatusCommand: {Command}", command);
            return Result<PatchPaymentStatusResult>.Failure("An error occurred while patching the payment status.");
        }
    }
}