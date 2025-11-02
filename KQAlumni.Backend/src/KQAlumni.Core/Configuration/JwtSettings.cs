namespace KQAlumni.Core.Configuration;

/// <summary>
/// Configuration settings for JWT authentication
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key for signing JWT tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (e.g., "KQAlumniAPI")
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT token audience (e.g., "KQAlumniAdmin")
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes (default: 480 = 8 hours)
    /// </summary>
    public int ExpirationMinutes { get; set; } = 480;
}
