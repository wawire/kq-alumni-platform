using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KQAlumni.Core.Configuration;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for admin user authentication and JWT token management
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates an admin user and generates JWT token
    /// </summary>
    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        const int MaxFailedAttempts = 5;
        const int LockoutMinutes = 30;

        try
        {
            // Find admin user by username
            var adminUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (adminUser == null)
            {
                _logger.LogWarning("Authentication failed: User {Username} not found or inactive", username);
                return null;
            }

            // Check if account is locked out
            if (adminUser.IsLockedOut)
            {
                var remainingMinutes = (adminUser.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes;
                _logger.LogWarning(
                    "Authentication failed: Account {Username} is locked out for {Minutes:F1} more minutes",
                    username,
                    remainingMinutes);
                throw new InvalidOperationException(
                    $"Account is locked due to multiple failed login attempts. Please try again in {Math.Ceiling(remainingMinutes)} minutes.");
            }

            // Verify password
            if (!VerifyPassword(password, adminUser.PasswordHash))
            {
                // Increment failed attempts
                adminUser.FailedLoginAttempts++;

                // Lock account if max attempts reached
                if (adminUser.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    adminUser.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    await _context.SaveChangesAsync();

                    _logger.LogWarning(
                        "Account {Username} locked out after {Attempts} failed attempts",
                        username,
                        adminUser.FailedLoginAttempts);

                    throw new InvalidOperationException(
                        $"Account locked due to {MaxFailedAttempts} failed login attempts. Please try again in {LockoutMinutes} minutes.");
                }

                await _context.SaveChangesAsync();

                var attemptsRemaining = MaxFailedAttempts - adminUser.FailedLoginAttempts;
                _logger.LogWarning(
                    "Authentication failed: Invalid password for user {Username}. {Remaining} attempts remaining",
                    username,
                    attemptsRemaining);

                throw new InvalidOperationException(
                    $"Invalid username or password. {attemptsRemaining} attempts remaining before account lockout.");
            }

            // Reset failed attempts on successful login
            if (adminUser.FailedLoginAttempts > 0 || adminUser.LockoutEnd.HasValue)
            {
                adminUser.FailedLoginAttempts = 0;
                adminUser.LockoutEnd = null;
            }

            // Update last login timestamp
            adminUser.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate and return JWT token
            var token = GenerateJwtToken(adminUser);
            _logger.LogInformation("User {Username} authenticated successfully", username);

            return token;
        }
        catch (InvalidOperationException)
        {
            // Re-throw lockout and authentication errors
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Generates a JWT token for an admin user
    /// </summary>
    public string GenerateJwtToken(AdminUser adminUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, adminUser.Username),
            new Claim(ClaimTypes.Email, adminUser.Email),
            new Claim(ClaimTypes.Role, adminUser.Role),
            new Claim("FullName", adminUser.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }

    /// <summary>
    /// Gets admin user by username
    /// </summary>
    public async Task<AdminUser?> GetAdminUserByUsernameAsync(string username)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    /// <summary>
    /// Creates a new admin user
    /// </summary>
    public async Task<AdminUser> CreateAdminUserAsync(
        string username,
        string email,
        string password,
        string fullName,
        string role)
    {
        // Check if username already exists
        var existingUser = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"Username '{username}' already exists");
        }

        // Check if email already exists
        var existingEmail = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Email '{email}' already exists");
        }

        // Validate role
        var validRoles = new[] { "SuperAdmin", "HRManager", "HROfficer" };
        if (!validRoles.Contains(role))
        {
            throw new ArgumentException($"Invalid role: {role}. Must be one of: {string.Join(", ", validRoles)}");
        }

        // Create new admin user
        var adminUser = new AdminUser
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminUsers.Add(adminUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user {Username} created successfully with role {Role}", username, role);

        return adminUser;
    }

    /// <summary>
    /// Updates admin user's last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(int adminUserId)
    {
        var adminUser = await _context.AdminUsers.FindAsync(adminUserId);
        if (adminUser != null)
        {
            adminUser.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
