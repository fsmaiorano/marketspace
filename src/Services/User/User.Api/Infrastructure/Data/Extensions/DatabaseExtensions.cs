using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Data.Models;

namespace User.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    private static readonly SemaphoreSlim _migrationLock = new(1, 1);

    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        UserDbContext context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        await _migrationLock.WaitAsync();
        try
        {
            if (context.Database.IsRelational())
            {
                List<string> pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"User: Applying {pendingMigrations.Count()} pending migration(s)...");

                    int retries = 10;
                    while (retries > 0)
                    {
                        try
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("User database migration completed successfully.");
                            break;
                        }
                        catch (Exception ex) when (retries > 1)
                        {
                            retries--;
                            Console.WriteLine($"User migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("User database is up to date, no migrations needed.");
                }
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("User non-relational database ensured.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"User database initialization failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.WriteLine("Application will continue without database initialization.");
        }
        finally
        {
            _migrationLock.Release();
        }

        Console.WriteLine("Database initialization completed successfully.");

        RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (string roleName in new[] { "Admin", "Member" })
        {
            string normalized = roleName.ToUpperInvariant();
            List<IdentityRole> roles = await roleManager.Roles.Where(r => r.NormalizedName == normalized).ToListAsync();

            switch (roles.Count)
            {
                case 0:
                    Console.WriteLine($"Creating {roleName} role...");
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    continue;
                case > 1:
                    {
                        Console.WriteLine($"Found {roles.Count} roles named '{roleName}' in DB; removing duplicates...");
                        foreach (IdentityRole duplicate in roles.Skip(1))
                        {
                            await roleManager.DeleteAsync(duplicate);
                        }

                        break;
                    }
            }
        }

        Console.WriteLine("Role initialization completed successfully.");

        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@marketspace.com";
        const string adminPassword = "Password123!";

        string normalizedAdminEmail = userManager.NormalizeEmail(adminEmail);
        ApplicationUser? adminUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedAdminEmail);
        if (adminUser == null)
        {
            Console.WriteLine("Creating admin user...");
            adminUser = new ApplicationUser
            {
                UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, EnableNotifications = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"Admin user created successfully with email: {adminEmail}");
            }
            else
            {
                Console.WriteLine(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine("Admin user already exists.");
        }

        Console.WriteLine("User.Api initialization completed successfully.");
    }
}

