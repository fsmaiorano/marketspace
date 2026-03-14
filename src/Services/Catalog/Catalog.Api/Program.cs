using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services;
using BuildingBlocks.Services.Correlation;
using Catalog.Api.Application;
using Catalog.Api.Endpoints;
using Catalog.Api.Infrastructure;
using Catalog.Api.Infrastructure.Data;
using MarketSpace.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();

builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCorrelationIdServices();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitialiseDatabaseAsync<CatalogDbContext>();

// app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseExceptionHandler(options => { });

CreateCatalogEndpoint.MapEndpoint(app);
UpdateCatalogEndpoint.MapEndpoint(app);
DeleteCatalogEndpoint.MapEndpoint(app);
GetCatalogByIdEndpoint.MapEndpoint(app);
GetCatalogEndpoint.MapEndpoint(app);
GetCatalogByMerchantIdEndpoint.MapEndpoint(app);
UpdateStockEndpoint.MapEndpoint(app);

app.Run();

namespace Catalog.Api
{
    public partial class CatalogProgram;
}