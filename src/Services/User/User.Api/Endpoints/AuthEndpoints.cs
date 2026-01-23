using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Primitives;
using User.Api.Data;
using User.Api.Data.Models;
using User.Api.Models;
using User.Api.Services;

namespace User.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/auth");
        group.WithTags("Auth");

        group.MapPost("/register", Register)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/revoke", Revoke)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/me", Me)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Register(
        RegisterRequest dto,
        UserDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        HttpContext http,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        bool isInMemory = dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        IDbContextTransaction? transaction = isInMemory ? null : await dbContext.Database.BeginTransactionAsync();

        try
        {
            ApplicationUser? existing = await userManager.FindByEmailAsync(dto.Email);
            if (existing is not null)
            {
                logger.LogWarning("Registration failed: Email {Email} already exists", dto.Email);
                return Results.BadRequest(new { message = "E-mail already exists." });
            }

            ApplicationUser user = new()
            {
                UserName = dto.UserName ?? dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            IdentityResult identityResult = await userManager.CreateAsync(user, dto.Password);
            if (!identityResult.Succeeded)
            {
                string errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                logger.LogWarning("Registration failed for {Email}: {Errors}", dto.Email, errors);
                return Results.BadRequest(new
                {
                    message = "Registration failed",
                    errors = identityResult.Errors.Select(e => e.Description).ToList()
                });
            }

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, "Member");
            if (!addToRoleResult.Succeeded)
            {
                logger.LogError("Failed to assign role to user {Email}", dto.Email);
                return Results.BadRequest(new
                {
                    message = "Failed to assign role to user",
                    errors = addToRoleResult.Errors.Select(e => e.Description).ToList()
                });
            }

            if (transaction is not null)
                await transaction.CommitAsync();

            logger.LogInformation("User.Api {Email} registered successfully", dto.Email);
            AuthResponse tokens = await tokenService.CreateTokensAsync(user, GetIpAddress(http));
            return Results.Ok(tokens);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration for {Email}", dto.Email);
            if (transaction is not null)
                await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private static async Task<IResult> Login(
        AuthRequest dto,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        HttpContext http)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Results.Unauthorized();

        SignInResult res = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!res.Succeeded)
            return Results.Unauthorized();

        AuthResponse tokens = await tokenService.CreateTokensAsync(user, GetIpAddress(http));
        return Results.Ok(tokens);
    }

    private static async Task<IResult> Refresh(
        RefreshRequest dto,
        ITokenService tokenService,
        HttpContext http)
    {
        AuthResponse? response = await tokenService.RefreshAsync(dto.AccessToken, dto.RefreshToken, GetIpAddress(http));
        return response is null ? Results.BadRequest(new { message = "Invalid Token" }) : Results.Ok(response);
    }

    private static async Task<IResult> Revoke(
        RefreshRequest dto,
        ITokenService tokenService,
        HttpContext http)
    {
        await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, GetIpAddress(http));
        return Results.NoContent();
    }

    private static IResult Me(ClaimsPrincipal user)
    {
        string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userName = user.FindFirst(ClaimTypes.Name)?.Value;
        string? email = user.FindFirst(ClaimTypes.Email)?.Value;
        string? firstName = user.FindFirst(ClaimTypes.GivenName)?.Value;
        string? lastName = user.FindFirst(ClaimTypes.Surname)?.Value;
        return Results.Ok(new
        {
            userId,
            userName,
            email,
            firstName,
            lastName
        });
    }

    private static string GetIpAddress(HttpContext http)
    {
        if (http.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues value))
            return value.ToString();
        return http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}