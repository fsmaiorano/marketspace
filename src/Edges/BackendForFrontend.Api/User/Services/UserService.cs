using BackendForFrontend.Api.User.Contracts;
using BackendForFrontend.Api.User.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.User.Services;

public class UserService : IUserService
{
    private readonly IAppLogger<UserService> _logger;
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public UserService(
        IAppLogger<UserService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _http = httpClient;
        _baseUrl = configuration["Services:UserService:BaseUrl"] ?? throw new InvalidOperationException("UserService base url not configured");
    }

    private async Task<Result<T>> ExecuteAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        HttpResponseMessage res = await action();
        if (res.IsSuccessStatusCode && res.Content is not null)
        {
            T? body = await res.Content.ReadFromJsonAsync<T>();
            if (body is not null)
            {
                return Result<T>.Success(body);
            }
        }

        string error = await res.Content?.ReadAsStringAsync()! ?? "Unknown error";
        return Result<T>.Failure(error);
    }

    public Task<Result<AuthResponse>> LoginAsync(LoginRequest request) =>
        ExecuteAsync<AuthResponse>(() => _http.PostAsJsonAsync($"{_baseUrl}/api/auth/login", request));

    public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request) =>
        ExecuteAsync<AuthResponse>(() => _http.PostAsJsonAsync($"{_baseUrl}/api/auth/register", request));

    public Task<Result<MeResponse>> MeAsync() =>
        ExecuteAsync<MeResponse>(() => _http.GetAsync($"{_baseUrl}/api/auth/me"));

    public Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request) =>
        ExecuteAsync<AuthResponse>(() => _http.PostAsJsonAsync($"{_baseUrl}/api/auth/refresh", request));

    public async Task<Result<object>> RevokeAsync(RefreshRequest request)
    {
        HttpResponseMessage res = await _http.PostAsJsonAsync($"{_baseUrl}/api/auth/revoke", request);
        if (res.IsSuccessStatusCode) return Result<object>.Success(null!);
        string error = await res.Content?.ReadAsStringAsync()! ?? "Unknown error";
        return Result<object>.Failure(error);
    }

    public async Task<Result<object>> UpdateUserTypeAsync(UpdateUserTypeRequest request)
    {
        HttpResponseMessage res = await _http.PutAsJsonAsync($"{_baseUrl}/api/auth/update-user-type", request);
        if (res.IsSuccessStatusCode) return Result<object>.Success(null!);
        string error = await res.Content?.ReadAsStringAsync()! ?? "Unknown error";
        return Result<object>.Failure(error);
    }
}