using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using Catalog.Api.Application;
using Catalog.Api.Endpoints;
using Catalog.Api.Infrastructure;
using Catalog.Api.Infrastructure.Data.Extensions;
using Serilog.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services
    .AddObservability(builder.Configuration, options =>
    {
        options.ServiceName = "Catalog.Api";
        options.ServiceVersion = "1.0.0";
    });

builder.Services.AddSingleton<DiagnosticContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync();
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

CreateCatalogEndpoint.MapEndpoint(app);
UpdateCatalogEndpoint.MapEndpoint(app);
DeleteCatalogEndpoint.MapEndpoint(app);
GetCatalogByIdEndpoint.MapEndpoint(app);

app.Run();

namespace Catalog.Api
{
    public partial class CatalogProgram;
}