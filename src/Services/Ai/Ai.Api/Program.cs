using Ai.Api.Application;
using Ai.Api.Endpoints;
using Ai.Api.Infrastructure;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();
builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices()
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

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseExceptionHandler(options => { });

AgentEndpoint.MapEndpoints(app);
RagEndpoint.MapEndpoints(app);
ChatEndpoint.MapEndpoint(app);

app.Run();

namespace Ai.Api
{
    public partial class AiProgram;
}