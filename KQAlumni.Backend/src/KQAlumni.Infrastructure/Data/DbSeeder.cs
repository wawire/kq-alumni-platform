using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KQAlumni.Infrastructure.Data;

/// <summary>
/// Database seeding extensions for initial admin users
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds the initial admin users (SuperAdmin, HRManager, HROfficer) if no admin users exist
    /// This should be called during application startup in Development environment
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    public static async Task SeedInitialAdminUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Check if any admin users already exist
            var adminExists = await context.AdminUsers.AnyAsync();

            if (adminExists)
            {
                logger.LogInformation("Admin users already exist. Skipping seeding.");
                return;
            }

            logger.LogInformation("No admin users found. Creating initial admin users...");

            // Define the three initial admin users
            var adminUsers = new List<AdminUserSeedData>
            {
                new AdminUserSeedData
                {
                    Username = "admin",
                    Email = "admin@kenya-airways.com",
                    Password = "Admin@123456",
                    FullName = "System Administrator",
                    Role = "SuperAdmin",
                    RequiresPasswordChange = true
                },
                new AdminUserSeedData
                {
                    Username = "hr.manager",
                    Email = "hr.manager@kenya-airways.com",
                    Password = "HRManager@123456",
                    FullName = "HR Manager",
                    Role = "HRManager",
                    RequiresPasswordChange = true
                },
                new AdminUserSeedData
                {
                    Username = "hr.officer",
                    Email = "hr.officer@kenya-airways.com",
                    Password = "HROfficer@123456",
                    FullName = "HR Officer",
                    Role = "HROfficer",
                    RequiresPasswordChange = true
                }
            };

            // Create all admin users
            foreach (var adminData in adminUsers)
            {
                var adminUser = await authService.CreateAdminUserAsync(
                    adminData.Username,
                    adminData.Email,
                    adminData.Password,
                    adminData.FullName,
                    adminData.Role,
                    requiresPasswordChange: adminData.RequiresPasswordChange);

                logger.LogInformation(
                    "[SUCCESS] Admin user created: {Username} ({Role}) - Email: {Email}",
                    adminData.Username,
                    adminData.Role,
                    adminData.Email);
            }

            logger.LogInformation(
                "[SUCCESS] All {Count} initial admin users created successfully.\n" +
                "   [SECURITY] All users MUST change their passwords on first login.\n" +
                "   Credentials:\n" +
                "   1. SuperAdmin    - Username: admin        | Password: Admin@123456\n" +
                "   2. HRManager     - Username: hr.manager   | Password: HRManager@123456\n" +
                "   3. HROfficer     - Username: hr.officer   | Password: HROfficer@123456",
                adminUsers.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding initial admin users");
            throw;
        }
    }

    /// <summary>
    /// Seeds multiple admin users from a predefined list
    /// Use this for initial setup with multiple HR staff
    /// </summary>
    public static async Task SeedAdminUsersAsync(
        IServiceProvider serviceProvider,
        List<AdminUserSeedData> adminUsers)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            foreach (var adminData in adminUsers)
            {
                // Check if user already exists
                var existingUser = await context.AdminUsers
                    .FirstOrDefaultAsync(u => u.Username == adminData.Username || u.Email == adminData.Email);

                if (existingUser != null)
                {
                    logger.LogWarning(
                        "Admin user {Username} already exists. Skipping.",
                        adminData.Username);
                    continue;
                }

                // Create admin user with optional password change requirement
                var adminUser = await authService.CreateAdminUserAsync(
                    adminData.Username,
                    adminData.Email,
                    adminData.Password,
                    adminData.FullName,
                    adminData.Role,
                    requiresPasswordChange: adminData.RequiresPasswordChange);

                logger.LogInformation(
                    "[SUCCESS] Admin user created: {Username} ({Role})",
                    adminData.Username,
                    adminData.Role);
            }

            logger.LogInformation("Admin user seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding admin users");
            throw;
        }
    }
}

/// <summary>
/// Admin user seed data model
/// </summary>
public class AdminUserSeedData
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "HROfficer";
    public bool RequiresPasswordChange { get; set; } = false;
}
