using BackendForFrontend.Api.Aggregations.Contracts;
using BackendForFrontend.Api.Aggregations.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;

namespace BackendForFrontend.Api.Aggregations.Endpoints;

public static class AggregationEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/{customerId:guid}",
                async ([FromRoute] Guid customerId, [FromServices] ICustomerDashboardUseCase usecase) =>
                {
                    CustomerDashboardResponse result = await usecase.GetCustomerDashboardAsync(customerId);
                    return Results.Ok(Result<CustomerDashboardResponse>.Success(result));
                })
            .WithName("GetCustomerDashboard")
            .WithTags("Dashboard")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
