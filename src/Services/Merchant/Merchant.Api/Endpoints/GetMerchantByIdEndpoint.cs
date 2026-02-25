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
                        Name = result.Data.Name,
                        Description = result.Data.Description,
                        Email = result.Data.Email,
                        PhoneNumber = result.Data.PhoneNumber,
                        Address = result.Data.Address,
                        CreatedAt = result.Data.CreatedAt,
                        UpdatedAt = result.Data.UpdatedAt
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