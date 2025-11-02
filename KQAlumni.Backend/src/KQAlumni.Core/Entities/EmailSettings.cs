namespace KQAlumni.Core.Entities;

/// <summary>
/// Configuration settings for email service
/// Maps to "Email" section in appsettings.json
/// </summary>
public class EmailSettings
{
  public const string SectionName = "Email";

  /// <summary>
  /// SMTP server address
  /// </summary>
  public string SmtpServer { get; set; } = string.Empty;

  /// <summary>
  /// SMTP server port
  /// </summary>
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
  public string From { get; set; } = "KQ.Alumni@kenya-airways.com";

  /// <summary>
  /// From display name
  /// </summary>
  public string DisplayName { get; set; } = "Kenya Airways Alumni Relations";

  /// <summary>
  /// Timeout in seconds
  /// </summary>
  public int TimeoutSeconds { get; set; } = 30;

  /// <summary>
  /// Enable actual email sending (false for development)
  /// </summary>
  public bool EnableEmailSending { get; set; }

  /// <summary>
  /// Use mock email service for testing
  /// </summary>
  public bool UseMockEmailService { get; set; }
}
