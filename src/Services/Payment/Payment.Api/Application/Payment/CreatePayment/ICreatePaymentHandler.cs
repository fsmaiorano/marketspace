using BuildingBlocks;

namespace Payment.Api.Application.Payment.CreatePayment;

public interface ICreatePaymentHandler
{
    Task<Result<CreatePaymentResult>> HandleAsync(CreatePaymentCommand command);
}
