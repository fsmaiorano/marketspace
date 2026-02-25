using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Payment.GetPaymentById;
using Payment.Api.Endpoints.Dto;

namespace Payment.Api.Endpoints;

public static class GetPaymentByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/payment/{id}",
                async ([FromRoute] string id, [FromServices] GetPaymentById handler) =>
                {
                    Result<GetPaymentByIdResult> result =
                        await handler.HandleAsync(new GetPaymentByIdQuery(Guid.Parse(id)));
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new PaymentDto()
                        {
                            Id = result.Data.Payment.Id.Value,
                            OrderId = result.Data.Payment.OrderId,
                            Amount = result.Data.Payment.Amount,
                            Currency = result.Data.Payment.Currency,
                            Status = result.Data.Payment.Status.ToString(),
                            Method = result.Data.Payment.Method.Value,
                            Provider = result.Data.Payment.Provider,
                            ProviderTransactionId = result.Data.Payment.ProviderTransactionId,
                            AuthorizationCode = result.Data.Payment.AuthorizationCode,
                            CreatedAt = result.Data.Payment.CreatedAt!.Value,
                            LastModifiedAt = result.Data.Payment.LastModifiedAt
                        })
                        : Results.NotFound(result.Error);
                })
            .WithName("GetPaymentById")
            .WithTags("Payment")
            .Produces<Result<GetPaymentByIdResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}