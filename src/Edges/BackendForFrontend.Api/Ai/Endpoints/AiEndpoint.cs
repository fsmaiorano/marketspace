using BackendForFrontend.Api.Ai.Dtos;
using BackendForFrontend.Api.Ai.UseCases;
using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;

namespace BackendForFrontend.Api.Ai.Endpoints;

public static class AiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/chat",
                async ([FromBody] ChatRequest request, [FromServices] AiUseCase usecase) =>
                {
                    Result<ChatResponse> result = await usecase.ChatAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("AiChat")
            .WithTags("Ai")
            .Produces<ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/api/ai/agent",
                async ([FromBody] AgentRequest request, [FromServices] AiUseCase usecase) =>
                {
                    Result<AgentResponse> result = await usecase.AgentAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("AiAgent")
            .WithTags("Ai")
            .Produces<AgentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/api/ai/rag",
                async ([FromBody] RagRequest request, [FromServices] AiUseCase usecase) =>
                {
                    Result<RagResponse> result = await usecase.RagAsync(request);
                    return result.IsSuccess
                        ? Results.Ok(result)
                        : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("AiRag")
            .WithTags("Ai")
            .Produces<RagResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
