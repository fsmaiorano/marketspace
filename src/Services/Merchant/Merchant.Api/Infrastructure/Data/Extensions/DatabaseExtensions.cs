using Microsoft.EntityFrameworkCore;

namespace Merchant.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    private static readonly SemaphoreSlim _migrationLock = new(1, 1);
    
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        MerchantDbContext context = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

        // Use semaphore to prevent multiple instances from migrating simultaneously
        await _migrationLock.WaitAsync();
        try
        {
            // Check if database exists and if migrations are needed
            if (context.Database.IsRelational())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Merchant: Applying {pendingMigrations.Count()} pending migration(s)...");
                    
                    // Apply migrations with retry logic
                    int retries = 3;
                    while (retries > 0)
                    {
                        try
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("Merchant database migration completed successfully.");
                            break;
                        }
                        catch (Exception ex) when (retries > 1)
                        {
                            retries--;
                            Console.WriteLine($"Merchant migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                            await Task.Delay(TimeSpan.FromSeconds(2));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Merchant database is up to date, no migrations needed.");
                }
            }
            else
            {
                // For non-relational databases, ensure it's created
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Merchant non-relational database ensured.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Merchant database initialization failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Application will continue without database initialization.");
        }
        finally
        {
            _migrationLock.Release();
        }
    }
}