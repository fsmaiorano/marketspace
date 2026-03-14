using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;
using Merchant.Api.Application;
using Merchant.Api.Endpoints;
using Merchant.Api.Infrastructure;
using Merchant.Api.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();

builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices()
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

await app.InitialiseDatabaseAsync<MerchantDbContext>();

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

CreateMerchantEndpoint.MapEndpoint(app);
UpdateMerchantEndpoint.MapEndpoint(app);
DeleteMerchantEndpoint.MapEndpoint(app);
GetMerchantByIdEndpoint.MapEndpoint(app);
GetMerchantByUserIdEndpoint.MapEndpoint(app);

app.Run();

namespace Merchant.Api
{
    public partial class MerchantProgram;
}