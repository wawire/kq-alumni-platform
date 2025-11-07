using System.ComponentModel.DataAnnotations;

namespace KQAlumni.Core.Entities;

/// <summary>
/// Configuration settings for email service
/// Maps to "Email" section in appsettings.json
/// </summary>
public class EmailSettings : IValidatableObject
{
  public const string SectionName = "Email";

  /// <summary>
  /// SMTP server address
  /// </summary>
  [Required(ErrorMessage = "SMTP server is required")]
  public string SmtpServer { get; set; } = string.Empty;

  /// <summary>
  /// SMTP server port
  /// </summary>
  [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535")]
  public int SmtpPort { get; set; } = 587;

  /// <summary>
  /// Enable SSL/TLS
  /// </summary>
  public bool EnableSsl { get; set; } = true;

  /// <summary>
  /// SMTP username (if authentication required)
  /// </summary>
  public string? Username { get; set; }

  /// <summary>
  /// SMTP password (if authentication required)
  /// </summary>
  public string? Password { get; set; }

  /// <summary>
  /// From email address
  /// </summary>
  [Required(ErrorMessage = "From email address is required")]
  [EmailAddress(ErrorMessage = "From must be a valid email address")]
  public string From { get; set; } = "KQ.Alumni@kenya-airways.com";

  /// <summary>
  /// From display name
  /// </summary>
  [Required(ErrorMessage = "Display name is required")]
  public string DisplayName { get; set; } = "Kenya Airways Alumni Relations";

  /// <summary>
  /// Timeout in seconds
  /// </summary>
  [Range(5, 300, ErrorMessage = "Timeout must be between 5 and 300 seconds")]
  public int TimeoutSeconds { get; set; } = 30;

  /// <summary>
  /// Enable actual email sending (false for development)
  /// </summary>
  public bool EnableEmailSending { get; set; }

  /// <summary>
  /// Use mock email service for testing
  /// </summary>
  public bool UseMockEmailService { get; set; }

  /// <summary>
  /// Custom validation logic for Email settings
  /// </summary>
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    var results = new List<ValidationResult>();

    // If email sending is enabled, require username and password
    if (EnableEmailSending && !UseMockEmailService)
    {
      if (string.IsNullOrWhiteSpace(Username))
      {
        results.Add(new ValidationResult(
          "Username is required when EnableEmailSending is true and UseMockEmailService is false",
          new[] { nameof(Username) }));
      }

      if (string.IsNullOrWhiteSpace(Password))
      {
        results.Add(new ValidationResult(
          "Password is required when EnableEmailSending is true and UseMockEmailService is false",
          new[] { nameof(Password) }));
      }
    }

    // In production, ensure mock email service is disabled
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (environment == "Production" && UseMockEmailService)
    {
      results.Add(new ValidationResult(
        "Mock email service must be disabled in Production environment",
        new[] { nameof(UseMockEmailService) }));
    }

    return results;
  }
}
