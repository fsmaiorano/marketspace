using BackendForFrontend.Api.Ai.Dtos;
using BackendForFrontend.Api.Ai.UseCases;
using BuildingBlocks;
using BuildingBlocks.Services.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace BackendForFrontend.Api.Ai.Endpoints;

public static class AiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/chat",
                async ([FromBody] ChatRequest request, [FromServices] AiUseCase usecase,
                    [FromServices] ICurrentUserService currentUser) =>
                {
                    // Always use userId from the authenticated JWT — never trust client-provided value
                    ChatRequest enriched = request with { UserId = currentUser.UserId };
                    Result<ChatResponse> result = await usecase.ChatAsync(enriched);
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
                async ([FromBody] AgentRequest request, [FromServices] AiUseCase usecase,
                    [FromServices] ICurrentUserService currentUser) =>
                {
                    AgentRequest enriched = request with { UserId = currentUser.UserId };
                    Result<AgentResponse> result = await usecase.AgentAsync(enriched);
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
                async ([FromBody] RagRequest request, [FromServices] AiUseCase usecase,
                    [FromServices] ICurrentUserService currentUser) =>
                {
                    RagRequest enriched = request with { UserId = currentUser.UserId };
                    Result<RagResponse> result = await usecase.RagAsync(enriched);
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
