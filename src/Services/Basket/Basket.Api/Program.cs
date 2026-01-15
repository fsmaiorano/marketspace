using Basket.Api.Application;
using Basket.Api.Endpoints;
using Basket.Api.Infrastructure;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services.Correlation;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCustomLoggers();

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
}

// app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(options => { });

CreateBasketEndpoint.MapEndpoint(app);
GetBasketByIdEndpoint.MapEndpoint(app);
DeleteBasketEndpoint.MapEndpoint(app);
CheckoutBasketEndpoint.MapEndpoint(app);

app.Run();

namespace Basket.Api
{
    public partial class BasketProgram;
}