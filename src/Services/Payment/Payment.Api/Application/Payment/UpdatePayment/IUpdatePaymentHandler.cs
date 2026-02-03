using BuildingBlocks;

namespace Payment.Api.Application.Payment.UpdatePayment;

public interface IUpdatePaymentHandler
{
    Task<Result<UpdatePaymentResult>> HandleAsync(UpdatePaymentCommand command);
}
