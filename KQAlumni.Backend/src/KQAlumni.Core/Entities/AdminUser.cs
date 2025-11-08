namespace KQAlumni.Core.Entities;

/// <summary>
/// Represents an admin user with access to the HR dashboard
/// </summary>
public class AdminUser
{
    /// <summary>
    /// Unique identifier for the admin user
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username for login (unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address (unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (never store plain text)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Admin role: SuperAdmin, HRManager, HROfficer
    /// </summary>
    public string Role { get; set; } = "HROfficer";

    /// <summary>
    /// Full name of the admin user
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the admin account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the admin last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Who created this admin account
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Number of consecutive failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// UTC timestamp when account lockout expires (null if not locked)
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Checks if account is currently locked out
    /// </summary>
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    /// <summary>
    /// Forces the user to change their password on next login
    /// Used for initial seeded admin accounts with default passwords
    /// </summary>
    public bool RequiresPasswordChange { get; set; } = false;

    /// <summary>
    /// Navigation property: Audit logs performed by this admin
    /// </summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

/// <summary>
/// Admin roles with different permission levels
/// </summary>
public static class AdminRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HRManager = "HRManager";
    public const string HROfficer = "HROfficer";

    public static readonly string[] All = { SuperAdmin, HRManager, HROfficer };
}
