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
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/update-user-type", UpdateUserType)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
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
                Name = dto.Name,
                UserType = dto.UserType!.Value
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

    private static async Task<IResult> Me(ClaimsPrincipal user, UserManager<ApplicationUser> userManager)
    {
        string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userName = user.FindFirst(ClaimTypes.Name)?.Value;
        string? email = user.FindFirst(ClaimTypes.Email)?.Value;
        string? name = user.FindFirst(ClaimTypes.GivenName)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        ApplicationUser? applicationUser = await userManager.FindByIdAsync(userId);
        if (applicationUser is null)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            userId,
            userName,
            email,
            name = name ?? applicationUser.Name,
            userType = applicationUser.UserType.ToString(),
            enableNotifications = applicationUser.EnableNotifications
        });
    }

    private static async Task<IResult> UpdateUserType(
        UpdateUserTypeRequest dto,
        UserManager<ApplicationUser> userManager,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("UpdateUserType attempt for userId: {UserId}", dto.UserId);

        try
        {
            ApplicationUser? user = await userManager.FindByIdAsync(dto.UserId);
            if (user is null)
            {
                logger.LogWarning("UpdateUserType failed: User {UserId} not found", dto.UserId);
                return Results.NotFound(new { message = "User not found." });
            }

            if (!Enum.IsDefined(dto.UserType))
            {
                logger.LogWarning("UpdateUserType failed: Invalid UserType value {UserType}", dto.UserType);
                return Results.BadRequest(new { message = "Invalid user type." });
            }

            user.UserType = dto.UserType;
            IdentityResult result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogWarning("UpdateUserType failed for {UserId}: {Errors}", dto.UserId, errors);
                return Results.BadRequest(new
                {
                    message = "Update failed", errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            logger.LogInformation("User {UserId} type updated successfully to {UserType}", dto.UserId, dto.UserType);
            return Results.Ok(new
            {
                message = "User type updated successfully", userId = user.Id, userType = user.UserType
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during UpdateUserType for {UserId}", dto.UserId);
            throw;
        }
    }

    private static string GetIpAddress(HttpContext http)
    {
        if (http.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues value))
            return value.ToString();
        return http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}