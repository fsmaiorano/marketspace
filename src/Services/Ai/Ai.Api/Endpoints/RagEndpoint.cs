using Ai.Api.Application;
using Ai.Api.Endpoints.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Endpoints;

public static class RagEndpoint
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/rag", async ([FromBody] RagRequest request, [FromServices] AskWithRagUseCase useCase) =>
            {
                try
                {
                    RagResponse response = await useCase.AskAsync(request);
                    return Results.Ok(response);
                }
                catch (Exception exception)
                {
                    return Results.Problem(exception.Message);
                }
            })
            .WithName("RAG")
            .WithTags("RAG")
            .Produces<RagResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapPost("/rag/ingest",
                async ([FromBody] IngestRequest request, [FromServices] IngestDocumentsUseCase useCase) =>
                {
                    try
                    {
                        IngestResponse response = await useCase.IngestAsync(request);
                        return Results.Ok(response);
                    }
                    catch (Exception exception)
                    {
                        return Results.Problem(exception.Message);
                    }
                })
            .WithName("RAGIngest")
            .WithTags("RAG")
            .Produces<IngestResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}