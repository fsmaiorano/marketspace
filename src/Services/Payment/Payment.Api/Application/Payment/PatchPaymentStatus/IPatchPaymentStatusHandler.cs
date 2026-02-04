using BuildingBlocks;

namespace Payment.Api.Application.Payment.PatchPaymentStatus;

public interface IPatchPaymentStatusHandler
{
    Task<Result<PatchPaymentStatusResult>> HandleAsync(PatchPaymentStatusCommand command);
}