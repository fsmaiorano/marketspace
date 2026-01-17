using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using User.Data;
using User.Data.Models;

namespace User.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        try
        {
            if (context.Database.IsRelational())
            {
                Console.WriteLine("Checking database connection and pending migrations...");
                
                // Check if the database can be connected to
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    Console.WriteLine("Cannot connect to database. Creating database...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("Database created and migrations applied successfully.");
                }
                else
                {
                    // Check for pending migrations
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    var pendingMigrationsList = pendingMigrations.ToList();
                    
                    if (pendingMigrationsList.Any())
                    {
                        Console.WriteLine($"Applying {pendingMigrationsList.Count} pending migration(s)...");
                        foreach (var migration in pendingMigrationsList)
                        {
                            Console.WriteLine($"  - {migration}");
                        }
                        
                        try
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("Pending migrations applied successfully.");
                        }
                        catch (PostgresException pgEx) when (pgEx.SqlState == "42P07")
                        {
                            Console.WriteLine($"\nWarning: Table already exists - {pgEx.MessageText}");
                            Console.WriteLine("This indicates the migration history is out of sync with the actual database state.");
                            Console.WriteLine("\nTo fix this, run the following SQL command:");
                            Console.WriteLine($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{pendingMigrationsList.First()}', '10.0.0');");
                            Console.WriteLine("\nOr, to completely reset the database:");
                            Console.WriteLine("  docker compose down -v && docker compose up -d");
                            Console.WriteLine("\nApplication will continue with existing tables...");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Database is up to date. No pending migrations.");
                    }
                }
            }
            else
            {
                // For non-relational databases, ensure the database is created
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("Using non-relational database provider - database ensured created.");
            }
            
            Console.WriteLine("Database initialization completed successfully.");
            
            // Initialize roles
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                Console.WriteLine("Creating Admin role...");
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            
            if (!await roleManager.RoleExistsAsync("Member"))
            {
                Console.WriteLine("Creating Member role...");
                await roleManager.CreateAsync(new IdentityRole("Member"));
            }
            
            Console.WriteLine("Role initialization completed successfully.");
            
            // Initialize admin user
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            const string adminEmail = "admin@forum.com";
            const string adminPassword = "Admin123!";
            
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                Console.WriteLine("Creating admin user...");
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    EnableNotifications = true
                };
                
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"Admin user created successfully with email: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
            
            Console.WriteLine("User initialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.GetType().Name}: {ex.Message}");
            
            // Log inner exception if exists
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            
            Console.WriteLine("Application will continue without database initialization.");
            Console.WriteLine("Note: If tables already exist, you may need to:");
            Console.WriteLine("  1. Ensure __EFMigrationsHistory table exists and is up to date");
            Console.WriteLine("  2. Or drop and recreate the database");
            Console.WriteLine("  3. Or use: dotnet ef database update");
        }
    }
}