using BackendForFrontend.Api.User.Dtos;
using BackendForFrontend.Api.User.Services;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.User.UseCases;

public class UserUseCase(
    IAppLogger<UserUseCase> logger,
    UserService service)
{
    private readonly IAppLogger<UserUseCase> _logger = logger;
    private readonly UserService _service = service;

    public Task<AuthResponse> LoginAsync(LoginRequest request) =>
        _service.LoginAsync(request);

    public Task<AuthResponse> RegisterAsync(RegisterRequest request) =>
        _service.RegisterAsync(request);

    public Task<MeResponse> MeAsync() =>
        _service.MeAsync();

    public Task<AuthResponse> RefreshAsync(RefreshRequest request) =>
        _service.RefreshAsync(request);

    public Task<bool> RevokeAsync(RefreshRequest request) =>
        _service.RevokeAsync(request);

    public Task<bool> UpdateUserTypeAsync(UpdateUserTypeRequest request) =>
        _service.UpdateUserTypeAsync(request);
}