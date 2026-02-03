using BuildingBlocks;

namespace Payment.Api.Application.Payment.DeletePayment;

public interface IDeletePaymentHandler
{
    Task<Result<DeletePaymentResult>> HandleAsync(DeletePaymentCommand command);
}
