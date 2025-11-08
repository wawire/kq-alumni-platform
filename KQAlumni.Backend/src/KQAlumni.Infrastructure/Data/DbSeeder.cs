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
    /// Seeds the initial SuperAdmin user if no admin users exist
    /// This should be called during application startup in Development environment
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="username">SuperAdmin username (default: admin)</param>
    /// <param name="email">SuperAdmin email (default: admin@kenya-airways.com)</param>
    /// <param name="password">SuperAdmin password (default: Admin@123456)</param>
    /// <param name="fullName">SuperAdmin full name (default: System Administrator)</param>
    public static async Task SeedInitialAdminUserAsync(
        IServiceProvider serviceProvider,
        string username = "admin",
        string email = "admin@kenya-airways.com",
        string password = "Admin@123456",
        string fullName = "System Administrator")
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

            logger.LogInformation("No admin users found. Creating initial SuperAdmin user...");

            // Create initial SuperAdmin user with forced password change
            var adminUser = await authService.CreateAdminUserAsync(
                username,
                email,
                password,
                fullName,
                "SuperAdmin",
                requiresPasswordChange: true);

            logger.LogInformation(
                "[SUCCESS] Initial SuperAdmin user created successfully:\n" +
                "   Username: {Username}\n" +
                "   Email: {Email}\n" +
                "   Password: {Password}\n" +
                "   [SECURITY] User MUST change password on first login",
                username,
                email,
                password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding initial admin user");
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
