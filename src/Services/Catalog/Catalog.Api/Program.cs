using BuildingBlocks.Exceptions;
using Catalog.Api.Application;
using Catalog.Api.Endpoints;
using Catalog.Api.Infrastructure;
using Catalog.Api.Infrastructure.Data.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

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

app.UseExceptionHandler(options => { });

CreateCatalogEndpoint.MapEndpoint(app);
UpdateCatalogEndpoint.MapEndpoint(app);
DeleteCatalogEndpoint.MapEndpoint(app);
GetCatalogByIdEndpoint.MapEndpoint(app);

app.Run();

public partial class CatalogProgram;