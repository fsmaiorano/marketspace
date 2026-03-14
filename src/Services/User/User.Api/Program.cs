using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using BuildingBlocks.Services;
using BuildingBlocks.Services.Correlation;
using MarketSpace.ServiceDefaults;
using User.Api.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCustomLoggers();

builder.Services
    .AddServices(builder.Configuration)
    .AddCorrelationIdServices();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitialiseDatabaseAsync<UserDbContext>();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();

namespace User.Api.Api
{
    public partial class UserProgram
    {
    }
}