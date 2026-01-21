using Microsoft.EntityFrameworkCore;

namespace Basket.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        BasketDbContext context = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        try
        {
            bool created = await context.Database.EnsureCreatedAsync();

            if (!created)
            {
                try
                {
                    if (context.Database.IsRelational())
                    {
                        await context.Database.MigrateAsync();
                        Console.WriteLine("Basket database migration completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Using non-relational database provider - skipping migrations.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Basket migration failed, but continuing: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("Basket database was created successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Basket database initialization failed: {ex.Message}");
            Console.WriteLine("Application will continue without database initialization.");
        }
    }
}
