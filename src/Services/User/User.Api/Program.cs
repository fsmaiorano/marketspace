using BuildingBlocks.Services;
using User.Api.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddServices(builder.Configuration);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync<UserDbContext>();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapDefaultEndpoints();

app.Run();

namespace User.Api.Api
{
    public partial class UserProgram
    {
    }
}