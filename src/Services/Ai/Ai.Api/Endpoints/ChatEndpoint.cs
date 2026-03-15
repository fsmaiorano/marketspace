using Ai.Api.Application;
using Ai.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Endpoints;

public static class ChatEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat", async ([FromBody] ChatRequest request, [FromServices] ChatUseCase useCase) =>
            {
                try
                {
                    ChatResponse response = await useCase.ChatAsync(request);
                    return Results.Ok(response);
                }
                catch (Exception exception)
                {
                    return Results.Problem(exception.Message);
                }
            })
            .WithName("Chat")
            .WithTags("Chat")
            .Produces<ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}