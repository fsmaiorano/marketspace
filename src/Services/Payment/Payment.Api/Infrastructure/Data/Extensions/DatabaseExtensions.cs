using Microsoft.EntityFrameworkCore;

namespace Payment.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    private static readonly SemaphoreSlim _migrationLock = new(1, 1);

    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        PaymentDbContext context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        // Use semaphore to prevent multiple instances from migrating simultaneously
        await _migrationLock.WaitAsync();
        try
        {
            // Check if database exists and if migrations are needed
            if (context.Database.IsRelational())
            {
                // Ensure the database exists (but don't create schema yet)
                // This will only create the database, not the tables
                try
                {
                    bool canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        Console.WriteLine("Payment: Database does not exist, will be created during migration.");
                    }
                }
                catch (Exception)
                {
                    // Database doesn't exist, it will be created during migration
                    Console.WriteLine("Payment: Database will be created during migration.");
                }

                List<string> pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Payment: Applying {pendingMigrations.Count()} pending migration(s)...");

                    // Apply migrations with retry logic
                    int retries = 3;
                    while (retries > 0)
                    {
                        try
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("Payment database migration completed successfully.");
                            break;
                        }
                        catch (Exception ex) when (retries > 1)
                        {
                            retries--;
                            Console.WriteLine(
                                $"Payment migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                            await Task.Delay(TimeSpan.FromSeconds(2));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Payment database is up to date, no migrations needed.");
                }
            }
            else
            {
                // For non-relational databases, ensure it's created
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Payment non-relational database ensured.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Payment database initialization failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Application will continue without database initialization.");
        }
        finally
        {
            _migrationLock.Release();
        }
    }
}