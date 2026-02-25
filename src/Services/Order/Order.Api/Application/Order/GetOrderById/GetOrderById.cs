using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Endpoints.Dto;
using System.Collections.ObjectModel;

namespace Order.Api.Application.Order.GetOrderById;

public record GetOrderByIdQuery(Guid Id);

public record GetOrderByIdResult(OrderEntity Order);

public class GetOrderById(
    IOrderRepository repository,
    IAppLogger<GetOrderById> logger)
{
    public async Task<Result<GetOrderByIdResult>> HandleAsync(GetOrderByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get order by ID request for: {OrderId}",
                query.Id);
            OrderId orderId = OrderId.Of(query.Id);
            OrderEntity? order = await repository.GetByIdAsync(orderId, isTrackingEnabled: false);

            return order is not null
                ? Result<GetOrderByIdResult>.Success(new GetOrderByIdResult(order))
                : Result<GetOrderByIdResult>.Failure($"Order with ID {query.Id} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting order by ID.");
            return Result<GetOrderByIdResult>.Failure("An error occurred while processing your request.");
        }
    }

    private static string MaskCard(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            return "****";
        string last4 = cardNumber[^4..];
        return new string('*', Math.Max(0, cardNumber.Length - 4)) + last4;
    }
}