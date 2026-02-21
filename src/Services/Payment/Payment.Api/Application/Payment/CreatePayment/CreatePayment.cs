using BuildingBlocks;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Repositories;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Application.Payment.CreatePayment;

public record CreatePaymentCommand
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Method { get; init; }
    public string Provider { get; init; } = string.Empty;
}

public record CreatePaymentResult();

public sealed class CreatePayment(
    IPaymentRepository repository,
    IAppLogger<CreatePayment> logger
) 
{
    public async Task<Result<CreatePaymentResult>> HandleAsync(CreatePaymentCommand command)
    {
        try 
        {
             logger.LogInformation(LogTypeEnum.Application, 
                "Processing create payment request for Order: {OrderId}", command.OrderId);
                
             PaymentEntity payment = PaymentEntity.Create(
                command.OrderId,
                command.Amount,
                command.Currency,
                PaymentMethod.Of(command.Method.ToString()),
                command.Provider
             );
             
             int result = await repository.AddAsync(payment);
             
             if (result <= 0)
             {
                 logger.LogError(LogTypeEnum.Application, null, 
                    "Failed to persist payment for Order: {OrderId}", command.OrderId);
                 return Result<CreatePaymentResult>.Failure("Failed to create payment.");
             }
             
             logger.LogInformation(LogTypeEnum.Business,
                "Payment created successfully. PaymentId: {PaymentId}, OrderId: {OrderId}", 
                payment.Id.Value, command.OrderId);
                
             return Result<CreatePaymentResult>.Success(new CreatePaymentResult());
        }
        catch(Exception ex)
        {
             logger.LogError(LogTypeEnum.Application, ex, 
                "Error creating payment for Order: {OrderId}", command.OrderId);
             throw; 
        }
    }
}
