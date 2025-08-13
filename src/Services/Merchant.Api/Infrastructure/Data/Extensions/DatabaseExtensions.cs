using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

        try
        {
            var created = await context.Database.EnsureCreatedAsync();

            if (!created)
            {
                try
                {
                    await context.Database.MigrateAsync();
                    Console.WriteLine("Database migration completed successfully.");
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