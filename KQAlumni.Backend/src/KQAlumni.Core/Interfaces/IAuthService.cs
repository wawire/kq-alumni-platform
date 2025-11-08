using KQAlumni.Core.Entities;

namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for admin user authentication and JWT token management
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an admin user and generates JWT token
    /// </summary>
    /// <param name="username">Admin username</param>
    /// <param name="password">Admin password</param>
    /// <returns>JWT token if authentication successful, null otherwise</returns>
    Task<string?> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Generates a JWT token for an admin user
    /// </summary>
    /// <param name="adminUser">Admin user entity</param>
    /// <returns>JWT token string</returns>
    string GenerateJwtToken(AdminUser adminUser);

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hashedPassword">Hashed password</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Gets admin user by username
    /// </summary>
    /// <param name="username">Admin username</param>
    /// <returns>Admin user entity or null</returns>
    Task<AdminUser?> GetAdminUserByUsernameAsync(string username);

    /// <summary>
    /// Creates a new admin user
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="email">Email</param>
    /// <param name="password">Password</param>
    /// <param name="fullName">Full name</param>
    /// <param name="role">Role (SuperAdmin, HRManager, HROfficer)</param>
    /// <param name="requiresPasswordChange">Whether user must change password on first login</param>
    /// <returns>Created admin user</returns>
    Task<AdminUser> CreateAdminUserAsync(string username, string email, string password, string fullName, string role, bool requiresPasswordChange = false);

    /// <summary>
    /// Updates admin user's last login timestamp
    /// </summary>
    /// <param name="adminUserId">Admin user ID</param>
    Task UpdateLastLoginAsync(int adminUserId);

    /// <summary>
    /// Changes an admin user's password
    /// </summary>
    /// <param name="username">Admin username</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <returns>True if password changed successfully</returns>
    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
}
