using BackendForFrontend.Api;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using Serilog;
using Serilog.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services .AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services
    .AddObservability(builder.Configuration, options =>
    {
        options.ServiceName = "Merchant.Api";
        options.ServiceVersion = "1.0.0";
    });


builder.Host.UseSerilog();
builder.Services.AddSingleton<DiagnosticContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

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
app.UseObservability();
app.UseExceptionHandler(options => { });


app.Run();