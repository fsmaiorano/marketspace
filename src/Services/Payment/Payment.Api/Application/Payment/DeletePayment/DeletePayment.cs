using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.DeletePayment;

public record DeletePaymentCommand(Guid Id);

public record DeletePaymentResult(bool Success);

public sealed class DeletePayment(
    IPaymentRepository repository,
    IAppLogger<DeletePayment> logger
) 
{
    public async Task<Result<DeletePaymentResult>> HandleAsync(DeletePaymentCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, 
                "Processing delete payment request for Payment: {PaymentId}", command.Id);

            int result = await repository.RemoveAsync(PaymentId.Of(command.Id));
             
            if (result <= 0)
            {
                logger.LogWarning(LogTypeEnum.Application, 
                   "Payment not found or failed to delete: {PaymentId}", command.Id);
                return Result<DeletePaymentResult>.Failure("Payment not found or delete failed.");
            }
             
            logger.LogInformation(LogTypeEnum.Business,
               "Payment deleted successfully. PaymentId: {PaymentId}", command.Id);

            return Result<DeletePaymentResult>.Success(new DeletePaymentResult(true));
        }
        catch(Exception ex)
        {
             logger.LogError(LogTypeEnum.Application, ex, 
                "Error deleting payment: {PaymentId}", command.Id);
             throw; 
        }
    }
}
