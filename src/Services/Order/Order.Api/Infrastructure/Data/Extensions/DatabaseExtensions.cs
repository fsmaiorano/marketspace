using Microsoft.EntityFrameworkCore;

namespace Order.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        OrderDbContext context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        try
        {
            bool created = await context.Database.EnsureCreatedAsync();

            if (!created)
            {
                try
                {
                    // Only run migrations if using a relational database provider
                    if (context.Database.IsRelational())
                    {
                        await context.Database.MigrateAsync();
                        Console.WriteLine("Database migration completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Using non-relational database provider - skiphahaping migrations.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration failed, but continuing: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("Database was created successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            Console.WriteLine("Application will continue without database initialization.");
        }
    }
}