using BackendForFrontend.Api.Merchant.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BackendForFrontend.Api;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddUseCases(services);
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services, configuration);
    }

    private static void AddUseCases(IServiceCollection services)
    {
        
    }

    private static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        string? userManagementApiUrl = configuration["ApiSettings:UserManagementApiUrl"];
        services.AddHttpClient<IMerchantService, MerchantService>(client =>
            {
                client.BaseAddress = new Uri(userManagementApiUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler(() =>
                new LoggingHandler(services.BuildServiceProvider().GetRequiredService<ILogger<LoggingHandler>>()));
        // .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    }

    public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to {Url}", request.RequestUri);

                var response = await base.SendAsync(request, cancellationToken);
                logger.LogInformation("Received response with status code {StatusCode}", response.StatusCode);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending the request.");
                throw;
            }
        }
    }
}