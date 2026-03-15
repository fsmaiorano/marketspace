namespace Ai.Api.Endpoints;

public static class RagEndpoint
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/rag", () =>
            {
                try
                {
                    return Task.FromResult(Results.Ok("RAG response"));
                }
                catch (Exception exception)
                {
                    return Task.FromException<IResult>(exception);
                }
            })
            .WithName("RAG")
            .WithTags("RAG")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}