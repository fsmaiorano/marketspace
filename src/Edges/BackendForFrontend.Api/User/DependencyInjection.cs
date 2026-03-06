using BackendForFrontend.Api.User.Services;
using BackendForFrontend.Api.User.UseCases;

namespace BackendForFrontend.Api.User;

public static class DependencyInjection
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<UserUseCase>();
        services.AddHttpClient<UserService>();
        return services;
    }
}