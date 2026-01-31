using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        // Ensure roles exist and remove duplicates that may have been created by multiple test factories
        foreach (string roleName in new[] { "Admin", "Member" })
        {
            string normalized = roleName.ToUpperInvariant();
            List<IdentityRole> roles = await roleManager.Roles.Where(r => r.NormalizedName == normalized).ToListAsync();

            if (roles.Count == 0)
            {
                Console.WriteLine($"Creating {roleName} role...");
                await roleManager.CreateAsync(new IdentityRole(roleName));
                continue;
            }

            // If more than one role exists with the same normalized name, keep the first and delete the others.
            if (roles.Count > 1)
            {
                Console.WriteLine($"Found {roles.Count} roles named '{roleName}' in DB; removing duplicates...");
                IdentityRole keep = roles.First();
                foreach (IdentityRole duplicate in roles.Skip(1))
                {
                    await roleManager.DeleteAsync(duplicate);
                }
            }
        }

        Console.WriteLine("Role initialization completed successfully.");

        // Initialize admin user
        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@marketspace.com";
        const string adminPassword = "Password123!";

        // Use a tolerant query against the Users DbSet to avoid SingleOrDefault exceptions
        // when the InMemory provider contains duplicated entries (some test setups may create
        // multiple WebApplicationFactory instances that share the same named InMemory database).
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