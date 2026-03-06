using BackendForFrontend.Api.User.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;
using BackendForFrontend.Api.User.UseCases;

namespace BackendForFrontend.Api.User.Endpoints;

public static class UserEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login",
                async ([FromBody] LoginRequest req, [FromServices] UserUseCase usecase) =>
                {
                    AuthResponse result = await usecase.LoginAsync(req);
                    return result != null ? Results.Ok(result) : Results.BadRequest("Login failed");
                })
            .WithName("UserLogin")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/register",
                async ([FromBody] RegisterRequest req, [FromServices] UserUseCase usecase) =>
                {
                    AuthResponse result = await usecase.RegisterAsync(req);
                    return result != null ? Results.Ok(result) : Results.BadRequest("Registration failed");
                })
            .WithName("UserRegister")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/auth/me",
                async ([FromServices] UserUseCase usecase) =>
                {
                    MeResponse result = await usecase.MeAsync();
                    return result != null ? Results.Ok(result) : Results.BadRequest("Failed to retrieve user information");
                })
            .RequireAuthorization()
            .WithName("UserMe")
            .WithTags("User")
            .Produces<MeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/refresh",
                async ([FromBody] RefreshRequest req, [FromServices] UserUseCase usecase) =>
                {
                    AuthResponse result = await usecase.RefreshAsync(req);
                    return result != null ? Results.Ok(result) : Results.BadRequest("Failed to refresh token");
                })
            .RequireAuthorization()
            .WithName("UserRefresh")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/auth/revoke",
                async ([FromBody] RefreshRequest req, [FromServices] UserUseCase usecase) =>
                {
                    bool result = await usecase.RevokeAsync(req);
                    return result ? Results.NoContent() : Results.BadRequest("Failed to revoke user session");
                })
            .RequireAuthorization()
            .WithName("UserRevoke")
            .WithTags("User")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPut("/api/auth/update-user-type",
                async ([FromBody] UpdateUserTypeRequest req, [FromServices] UserUseCase usecase) =>
                {
                    bool result = await usecase.UpdateUserTypeAsync(req);
                    return result ? Results.Ok() : Results.BadRequest("Failed to update user type");
                })
            .RequireAuthorization()
            .WithName("UserUpdateType")
            .WithTags("User")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}