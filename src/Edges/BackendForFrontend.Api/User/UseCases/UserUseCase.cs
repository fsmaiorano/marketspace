using BackendForFrontend.Api.User.Contracts;
using BackendForFrontend.Api.User.Dtos;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace BackendForFrontend.Api.User.UseCases;

public class UserUseCase(
    IAppLogger<UserUseCase> logger,
    IUserService service) : IUserUseCase
{
  private readonly IAppLogger<UserUseCase> _logger = logger;
  private readonly IUserService _service = service;

  public Task<Result<AuthResponse>> LoginAsync(LoginRequest request) =>
      _service.LoginAsync(request);

  public Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request) =>
      _service.RegisterAsync(request);

  public Task<Result<MeResponse>> MeAsync() =>
      _service.MeAsync();

  public Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request) =>
      _service.RefreshAsync(request);

  public Task<Result<object>> RevokeAsync(RefreshRequest request) =>
      _service.RevokeAsync(request);

  public Task<Result<object>> UpdateUserTypeAsync(UpdateUserTypeRequest request) =>
      _service.UpdateUserTypeAsync(request);
}