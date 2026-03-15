using Ai.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Endpoints;

public static class ChatEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat", ([FromBody] ChatRequest request) =>
            {
                try
                {
                    ChatResponse response = new ChatResponse { Answer = $"You said: {request.Message}" };
                    return Task.FromResult(Results.Ok(response));
                }
                catch (Exception exception)
                {
                    return Task.FromException<IResult>(exception);
                }
            })
            .WithName("Chat")
            .WithTags("Chat")
            .Produces<ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}