using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Api.Endpoints.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class GetMerchantByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/merchant/{id:guid}", async ([FromRoute] Guid id, [FromServices] GetMerchantById handler) =>
            {
                GetMerchantByIdQuery query = new(id);
                Result<GetMerchantByIdResult> result = await handler.HandleAsync(query);
                return result is { IsSuccess: true, Data: not null }
                    ? Results.Ok(new MerchantDto()
                    {
                        Name = result.Data.Merchant.Name,
                        Description = result.Data.Merchant.Description,
                        Email = result.Data.Merchant.Email.Value,
                        PhoneNumber = result.Data.Merchant.PhoneNumber,
                        Address = result.Data.Merchant.Address,
                        CreatedAt = result.Data.Merchant.CreatedAt,
                        UpdatedAt = result.Data.Merchant.UpdatedAt
                    })
                    : Results.NotFound(result.Error);
            })
            .WithName("GetMerchantById")
            .WithTags("Merchant")
            .Produces<GetMerchantByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}