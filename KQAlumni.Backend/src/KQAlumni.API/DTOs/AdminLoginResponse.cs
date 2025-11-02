namespace KQAlumni.API.DTOs;

/// <summary>
/// Response DTO for successful admin login
/// </summary>
public class AdminLoginResponse
{
    /// <summary>
    /// JWT token for authentication
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Admin user ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Admin username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Admin full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Admin role (SuperAdmin, HRManager, HROfficer)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Admin email
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
