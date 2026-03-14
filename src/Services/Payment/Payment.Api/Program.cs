using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;
using Payment.Api.Application;
using Payment.Api.Endpoints;
using Payment.Api.Infrastructure;
using Payment.Api.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();

builder.Services.AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCorrelationIdServices();

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitialiseDatabaseAsync<PaymentDbContext>();

//app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseExceptionHandler(options => { });

CreatePaymentEndpoint.MapEndpoint(app);
GetPaymentByIdEndpoint.MapEndpoint(app);
UpdatePaymentEndpoint.MapEndpoint(app);
PatchPaymentStatusEndpoint.MapEndpoint(app);
DeletePaymentEndpoint.MapEndpoint(app);

app.Run();

namespace Payment.Api
{
    public partial class PaymentProgram;
}