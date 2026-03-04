using BackendForFrontend.Api.User.Contracts;
using BackendForFrontend.Api.User.Services;
using BackendForFrontend.Api.User.UseCases;

namespace BackendForFrontend.Api.User;

public static class DependencyInjection
{
  public static IServiceCollection AddUserServices(this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddScoped<IUserUseCase, UserUseCase>();
    services.AddHttpClient<IUserService, UserService>();
    return services;
  }
}