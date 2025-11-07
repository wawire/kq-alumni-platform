using System.ComponentModel.DataAnnotations;

namespace KQAlumni.Core.Configuration;

/// <summary>
/// Configuration settings for JWT authentication
/// </summary>
public class JwtSettings : IValidatableObject
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key for signing JWT tokens
    /// </summary>
    [Required(ErrorMessage = "JWT SecretKey is required")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters for security")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (e.g., "KQAlumniAPI")
    /// </summary>
    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT token audience (e.g., "KQAlumniAdmin")
    /// </summary>
    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes (default: 480 = 8 hours)
    /// </summary>
    [Range(5, 43200, ErrorMessage = "ExpirationMinutes must be between 5 minutes and 30 days (43200 minutes)")]
    public int ExpirationMinutes { get; set; } = 480;

    /// <summary>
    /// Custom validation logic for JWT settings
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Warn if production is using weak secret key
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Production")
        {
            if (SecretKey.Length < 64)
            {
                results.Add(new ValidationResult(
                    "JWT SecretKey should be at least 64 characters in Production for enhanced security",
                    new[] { nameof(SecretKey) }));
            }

            // Check for common weak patterns
            if (SecretKey.Contains("dev") || SecretKey.Contains("test") || SecretKey.Contains("local"))
            {
                results.Add(new ValidationResult(
                    "JWT SecretKey appears to contain development/test keywords - use a strong production secret",
                    new[] { nameof(SecretKey) }));
            }
        }

        return results;
    }
}
