using Ai.Api.Application;
using Ai.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Endpoints;

public static class AgentEndpoint
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/agent", async ([FromBody] AgentRequest request, [FromServices] AgentUseCase useCase) =>
            {
                try
                {
                    AgentResponse response = await useCase.AgentAsync(request);
                    return Results.Ok(response);
                }
                catch (Exception exception)
                {
                    return Results.Problem(exception.Message);
                }
            })
            .WithName("Agent")
            .WithTags("Agent")
            .Produces<AgentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}