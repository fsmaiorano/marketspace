using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using User.Api.Data;
using User.Api.Data.Models;

namespace User.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        UserDbContext context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

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
                        Console.WriteLine("Using non-relational database provider - skipping migrations.");
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

        Console.WriteLine("Database initialization completed successfully.");

        // Initialize roles
        RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@forum.com";
        const string adminPassword = "Admin123!";

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
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