using WebApp.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

string? bffUrl = builder.Configuration["Services:BackendForFrontendService:BaseUrl"];
builder.Services.AddHttpClient<IMarketSpaceService, MarketSpaceService>(client =>
{
    client.BaseAddress = new Uri(bffUrl ?? throw new InvalidOperationException());
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();