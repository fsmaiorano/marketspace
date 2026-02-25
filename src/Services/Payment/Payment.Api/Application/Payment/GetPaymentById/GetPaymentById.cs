using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id);

public record GetPaymentByIdResult(PaymentEntity Payment);

public sealed class GetPaymentById(
    IPaymentRepository repository,
    IAppLogger<GetPaymentById> logger
)
{
    public async Task<Result<GetPaymentByIdResult>> HandleAsync(GetPaymentByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing get payment by id request: {PaymentId}", query.Id);

            PaymentEntity? payment = await repository.GetByIdAsync(PaymentId.Of(query.Id), isTrackingEnabled: false);

            return payment == null
                ? Result<GetPaymentByIdResult>.Failure("Payment not found.")
                : Result<GetPaymentByIdResult>.Success(new GetPaymentByIdResult(payment));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application, ex,
                "Error retrieving payment: {PaymentId}", query.Id);
            throw;
        }
    }
}