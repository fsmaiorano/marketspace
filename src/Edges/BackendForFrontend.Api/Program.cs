using BackendForFrontend.Api;
using BackendForFrontend.Api.Merchant;
using BackendForFrontend.Api.Merchant.Endpoints;
using BackendForFrontend.Api.Basket;
using BackendForFrontend.Api.Basket.Endpoints;
using BackendForFrontend.Api.Catalog;
using BackendForFrontend.Api.Catalog.Endpoints;
using BackendForFrontend.Api.Order;
using BackendForFrontend.Api.Order.Endpoints;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Loggers;
using BuildingBlocks.Middlewares;
using MarketSpace.ServiceDefaults;
using Serilog;
using Serilog.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMerchantServices(builder.Configuration);
builder.Services.AddBasketServices(builder.Configuration);
builder.Services.AddCatalogServices(builder.Configuration);
builder.Services.AddOrderServices(builder.Configuration);
builder.Services.AddCustomLoggers();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

IConfigurationSection jwtConfig = builder.Configuration.GetSection("Jwt");
string? issuer = jwtConfig["Issuer"];
string? audience = jwtConfig["Audience"];
string? secretKey = jwtConfig["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Invalid or expired token\"}");
        }
    };
});

builder.Host.UseSerilog();
builder.Services.AddSingleton<DiagnosticContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

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
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

MerchantEndpoint.MapEndpoint(app);
BasketEndpoint.MapEndpoint(app);
CatalogEndpoint.MapEndpoint(app);
OrderEndpoint.MapEndpoint(app);

app.Run();

namespace BackendForFrontend.Api
{
    public partial class BackendForFrontendProgram;
}