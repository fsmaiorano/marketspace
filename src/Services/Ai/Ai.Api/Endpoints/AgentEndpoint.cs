namespace Ai.Api.Endpoints;

public static class AgentEndpoint
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/agent", () =>
            {
                try
                {
                    return Task.FromResult(Results.Ok("Agent response"));
                }
                catch (Exception exception)
                {
                    return Task.FromException<IResult>(exception);
                }
            })
            .WithName("Agent")
            .WithTags("Agent")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}