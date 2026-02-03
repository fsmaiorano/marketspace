using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Application.Dto;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.GetPaymentById;

public sealed class GetPaymentByIdHandler(
    IPaymentRepository repository,
    IAppLogger<GetPaymentByIdHandler> logger
) : IGetPaymentByIdHandler
{
    public async Task<Result<GetPaymentByIdResult>> HandleAsync(GetPaymentByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing get payment by id request: {PaymentId}", query.Id);

            PaymentEntity? payment = await repository.GetByIdAsync(PaymentId.Of(query.Id), isTrackingEnabled: false);

            if (payment == null)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Payment not found: {PaymentId}", query.Id);
                return Result<GetPaymentByIdResult>.Failure("Payment not found.");
            }

            PaymentDto paymentDto = new()
            {
                Id = payment.Id.Value,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status.ToString(),
                Method = payment.Method.Value,
                Provider = payment.Provider,
                ProviderTransactionId = payment.ProviderTransactionId,
                AuthorizationCode = payment.AuthorizationCode,
                CreatedAt = payment.CreatedAt!.Value,
                LastModifiedAt = payment.LastModifiedAt
            };

            return Result<GetPaymentByIdResult>.Success(new GetPaymentByIdResult { Payment = paymentDto });
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex,
                "Error retrieving payment: {PaymentId}", query.Id);
            throw;
        }
    }
}