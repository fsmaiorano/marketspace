using BackendForFrontend.Api;
using BackendForFrontend.Api.Ai;
using BackendForFrontend.Api.Ai.Endpoints;
using BackendForFrontend.Api.Merchant;
using BackendForFrontend.Api.Merchant.Endpoints;
using BackendForFrontend.Api.Basket;
using BackendForFrontend.Api.Basket.Endpoints;
using BackendForFrontend.Api.Catalog;
using BackendForFrontend.Api.Catalog.Endpoints;
using BackendForFrontend.Api.Catalog.Subscribers;
using BackendForFrontend.Api.HostedService;
using BackendForFrontend.Api.MerchantDashboard;
using BackendForFrontend.Api.Order;
using BackendForFrontend.Api.Order.Endpoints;
using BackendForFrontend.Api.Services;
using BackendForFrontend.Api.User;
using BackendForFrontend.Api.User.Endpoints;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddCorrelationIdServices();

builder.Services.AddMerchantServices();
builder.Services.AddBasketServices();
builder.Services.AddCatalogServices();
builder.Services.AddOrderServices();
builder.Services.AddUserServices();
builder.Services.AddAiServices();
builder.Services.AddSingleton<IStockEventService, StockEventService>();
builder.Services.AddSingleton<IMerchantAlertService, MerchantAlertService>();
builder.Services.AddSingleton<IMerchantUserMappingService, MerchantUserMappingService>();

builder.Services.AddEventBus(builder.Configuration);
builder.Services.AddScoped<OnCatalogStockUpdatedSubscriber>();
builder.Services.AddScoped<OnStockReservationFailedSubscriber>();
builder.Services.AddHostedService<IntegrationEventSubscriptionService>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"), []
        }
    });
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseMiddleware<JwtTokenMiddleware>();
app.UseExceptionHandler(options => { });
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

MerchantEndpoint.MapEndpoint(app);
BasketEndpoint.MapEndpoint(app);
CatalogEndpoint.MapEndpoint(app);
OrderEndpoint.MapEndpoint(app);
UserEndpoint.MapEndpoint(app);
MerchantDashboardEndpoint.MapEndpoint(app);
AiEndpoint.MapEndpoint(app);

app.Run();

namespace BackendForFrontend.Api
{
    public partial class BackendForFrontendProgram;
}