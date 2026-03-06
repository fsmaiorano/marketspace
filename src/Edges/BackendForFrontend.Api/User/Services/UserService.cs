using BackendForFrontend.Api.Base;
using BackendForFrontend.Api.User.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.User.Services;

public class UserService(
    IAppLogger<UserService> logger,
    HttpClient httpClient,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor)
    : BaseService(httpClient)
{
    private string BaseUrl => configuration["Services:UserService:BaseUrl"] ??
                              throw new ArgumentNullException($"UserService BaseUrl is not configured");

    private string GetBearerToken()
    {
        string? authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..];
        }
        return string.Empty;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Logging in user with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/api/auth/login", request);
        AuthResponse? content =
            await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "User logged in successfully: {@User}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to login user. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error logging in user: {errorMessage}");
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Registering user with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/api/auth/register", request);
        AuthResponse? content =
            await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "User registered successfully: {@User}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to register user. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error registering user: {errorMessage}");
        }
    }

    public async Task<MeResponse> MeAsync()
    {
        logger.LogInformation(LogTypeEnum.Application, "Retrieving user information");

        string token = GetBearerToken();
        HttpResponseMessage response = await DoGet($"{BaseUrl}/api/auth/me", token);
        MeResponse? content =
            await response.Content.ReadFromJsonAsync<MeResponse>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Application, "User information retrieved successfully: {@User}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to retrieve user information. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving user information: {errorMessage}");
        }
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Refreshing user token with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/api/auth/refresh", request);
        AuthResponse? content =
            await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (response.IsSuccessStatusCode && content is not null)
        {
            logger.LogInformation(LogTypeEnum.Business, "User token refreshed successfully: {@User}", content);
            return content;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to refresh user token. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error refreshing user token: {errorMessage}");
        }
    }

    public async Task<bool> RevokeAsync(RefreshRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Revoking user session with request: {@Request}", request);

        HttpResponseMessage response = await DoPost($"{BaseUrl}/api/auth/revoke", request);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(LogTypeEnum.Business, "User session revoked successfully");
            return true;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to revoke user session. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error revoking user session: {errorMessage}");
        }
    }

    public async Task<bool> UpdateUserTypeAsync(UpdateUserTypeRequest request)
    {
        logger.LogInformation(LogTypeEnum.Application, "Updating user type with request: {@Request}", request);

        string token = GetBearerToken();
        HttpResponseMessage response = await DoPut($"{BaseUrl}/api/auth/update-user-type", request, token);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(LogTypeEnum.Business, "User type updated successfully");
            return true;
        }
        else
        {
            logger.LogError(LogTypeEnum.Application, null, "Failed to update user type. Status code: {StatusCode}, Response: {@Response}",
                response.StatusCode, response.Content);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating user type: {errorMessage}");
        }
    }
}