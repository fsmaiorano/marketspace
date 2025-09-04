using BackendForFrontend.Api;
using BackendForFrontend.Api.Merchant;
using BackendForFrontend.Api.Merchant.Endpoints;
using BackendForFrontend.Api.Basket;
using BackendForFrontend.Api.Basket.Endpoints;
using BackendForFrontend.Api.Catalog;
using BackendForFrontend.Api.Catalog.Endpoints;
using BackendForFrontend.Api.Order;
using BackendForFrontend.Api.Order.Endpoints;
using BackendForFrontend.Api.Aggregations;
using BackendForFrontend.Api.Aggregations.Endpoints;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using Serilog;
using Serilog.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMerchantServices(builder.Configuration);
builder.Services.AddBasketServices(builder.Configuration);
builder.Services.AddCatalogServices(builder.Configuration);
builder.Services.AddOrderServices(builder.Configuration);
builder.Services.AddAggregationServices(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services
    .AddObservability(builder.Configuration, options =>
    {
        options.ServiceName = "BackendForFrontend.Api";
        options.ServiceVersion = "1.0.0";
    });

builder.Host.UseSerilog();
builder.Services.AddSingleton<DiagnosticContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseObservability();
app.UseExceptionHandler(options => { });
app.UseCors("CorsPolicy");

MerchantEndpoint.MapEndpoint(app);
BasketEndpoint.MapEndpoint(app);
CatalogEndpoint.MapEndpoint(app);
OrderEndpoint.MapEndpoint(app);
AggregationEndpoint.MapEndpoint(app);

app.Run();

namespace BackendForFrontend.Api
{
    public partial class BackendForFrontendProgram;
}