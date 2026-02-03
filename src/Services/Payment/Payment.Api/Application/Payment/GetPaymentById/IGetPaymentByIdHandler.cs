using BuildingBlocks;

namespace Payment.Api.Application.Payment.GetPaymentById;

public interface IGetPaymentByIdHandler
{
    Task<Result<GetPaymentByIdResult>> HandleAsync(GetPaymentByIdQuery query);
}
