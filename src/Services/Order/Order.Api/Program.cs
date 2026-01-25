using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;
using Order.Api.Application;
using Order.Api.Endpoints;
using Order.Api.Infrastructure;
using Order.Api.Infrastructure.Data.Extensions;
using Serilog;
using Serilog.Extensions.Hosting;
using RabbitMQ.Client;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQClient("rabbitmq");

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCustomLoggers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Host.UseSerilog();
builder.Services.AddSingleton<DiagnosticContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync();
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

CreateOrderEndpoint.MapEndpoint(app);
UpdateOrderEndpoint.MapEndpoint(app);
DeleteOrderEndpoint.MapEndpoint(app);
GetOrderByIdEndpoint.MapEndpoint(app);

app.Run();

namespace Order.Api
{
    public partial class OrderProgram;
}