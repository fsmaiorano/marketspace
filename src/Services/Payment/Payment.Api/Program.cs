using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services.Correlation;
using Payment.Api.Application;
using Payment.Api.Endpoints;
using Payment.Api.Infrastructure;
using Payment.Api.Infrastructure.Data.Extensions;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCustomLoggers();

builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Host.UseSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync();
}

//app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
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