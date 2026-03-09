using BackendForFrontend.Api.User.Dtos;
using BackendForFrontend.Api.User.Services;

namespace BackendForFrontend.Api.User.UseCases;

public class UserUseCase(UserService service)
{
    public Task<AuthResponse> LoginAsync(LoginRequest request) =>
        service.LoginAsync(request);

    public Task<AuthResponse> RegisterAsync(RegisterRequest request) =>
        service.RegisterAsync(request);

    public Task<MeResponse> MeAsync() =>
        service.MeAsync();

    public Task<AuthResponse> RefreshAsync(RefreshRequest request) =>
        service.RefreshAsync(request);

    public Task<bool> RevokeAsync(RefreshRequest request) =>
        service.RevokeAsync(request);

    public Task<bool> UpdateUserTypeAsync(UpdateUserTypeRequest request) =>
        service.UpdateUserTypeAsync(request);
}