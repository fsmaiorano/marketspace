using BuildingBlocks.Exceptions;
using BuildingBlocks.Middlewares;
using Merchant.Api.Application;
using Merchant.Api.Endpoints;
using Merchant.Api.Infrastructure;
using Merchant.Api.Infrastructure.Data.Extensions;

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(options => { });

CreateMerchantEndpoint.MapEndpoint(app);
UpdateMerchantEndpoint.MapEndpoint(app);
DeleteMerchantEndpoint.MapEndpoint(app);
GetMerchantByIdEndpoint.MapEndpoint(app);

app.Run();

namespace Merchant.Api
{
    public partial class MerchantProgram;
}