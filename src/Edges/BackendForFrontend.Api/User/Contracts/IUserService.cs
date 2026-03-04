using BackendForFrontend.Api.User.Dtos;
using BuildingBlocks;

namespace BackendForFrontend.Api.User.Contracts;

public interface IUserService
{
  Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
  Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
  Task<Result<MeResponse>> MeAsync();
  Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request);
  Task<Result<object>> RevokeAsync(RefreshRequest request);
  Task<Result<object>> UpdateUserTypeAsync(UpdateUserTypeRequest request);
}