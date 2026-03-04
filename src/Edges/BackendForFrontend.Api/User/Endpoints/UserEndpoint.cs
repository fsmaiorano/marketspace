using BackendForFrontend.Api.User.Contracts;
using BackendForFrontend.Api.User.Dtos;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks;

namespace BackendForFrontend.Api.User.Endpoints;

public static class UserEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/user/login",
                async ([FromBody] LoginRequest req, [FromServices] IUserUseCase usecase) =>
                {
                    Result<AuthResponse> result = await usecase.LoginAsync(req);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("UserLogin")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/user/register",
                async ([FromBody] RegisterRequest req, [FromServices] IUserUseCase usecase) =>
                {
                    Result<AuthResponse> result = await usecase.RegisterAsync(req);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .WithName("UserRegister")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/user/me",
                async ([FromServices] IUserUseCase usecase) =>
                {
                    Result<MeResponse> result = await usecase.MeAsync();
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UserMe")
            .WithTags("User")
            .Produces<MeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/user/refresh",
                async ([FromBody] RefreshRequest req, [FromServices] IUserUseCase usecase) =>
                {
                    Result<AuthResponse> result = await usecase.RefreshAsync(req);
                    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UserRefresh")
            .WithTags("User")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/user/revoke",
                async ([FromBody] RefreshRequest req, [FromServices] IUserUseCase usecase) =>
                {
                    var result = await usecase.RevokeAsync(req);
                    return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UserRevoke")
            .WithTags("User")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPut("/api/user/update-user-type",
                async ([FromBody] UpdateUserTypeRequest req, [FromServices] IUserUseCase usecase) =>
                {
                    var result = await usecase.UpdateUserTypeAsync(req);
                    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
                })
            .RequireAuthorization()
            .WithName("UserUpdateType")
            .WithTags("User")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}